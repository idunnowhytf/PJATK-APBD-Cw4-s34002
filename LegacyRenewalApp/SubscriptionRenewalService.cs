using System;
using System.Collections.Generic;

namespace LegacyRenewalApp
{
    public class SubscriptionRenewalService
    {
        private readonly ICustomerRepository _customerRepository;
        private readonly ISubscriptionPlanRepository _planRepository;
        private readonly IBillingGateway _billingGateway;
        private readonly ITaxCalculator _taxCalculator;
        private readonly IFeeCalculator _feeCalculator;
        private readonly List<IDiscountRule> _discountRules;


        public SubscriptionRenewalService()
        {
            _customerRepository = new CustomerRepository();
            _planRepository = new SubscriptionPlanRepository();
            _billingGateway = new BillingGatewayAdapter();
            _taxCalculator = new TaxCalculator();
            _feeCalculator = new FeeCalculator();
            

            _discountRules = new List<IDiscountRule>
            {
                new SegmentDiscountRule(),
                new LoyaltyTimeDiscountRule(),
                new TeamSizeDiscountRule(),
                new LoyaltyPointsDiscountRule()
            };
        }


        public SubscriptionRenewalService(
            ICustomerRepository customerRepository,
            ISubscriptionPlanRepository planRepository,
            IBillingGateway billingGateway,
            ITaxCalculator taxCalculator,
            IFeeCalculator feeCalculator,
            List<IDiscountRule> discountRules)
        {
            _customerRepository = customerRepository;
            _planRepository = planRepository;
            _billingGateway = billingGateway;
            _taxCalculator = taxCalculator;
            _feeCalculator = feeCalculator;
            _discountRules = discountRules;
        }

        public RenewalInvoice CreateRenewalInvoice(
            int customerId,
            string planCode,
            int seatCount,
            string paymentMethod,
            bool includePremiumSupport,
            bool useLoyaltyPoints)
        {
            ValidateInputs(customerId, planCode, seatCount, paymentMethod);

            string normalizedPlanCode = planCode.Trim().ToUpperInvariant();
            string normalizedPaymentMethod = paymentMethod.Trim().ToUpperInvariant();


            var customer = _customerRepository.GetById(customerId);
            var plan = _planRepository.GetByCode(normalizedPlanCode);

            if (!customer.IsActive)
            {
                throw new InvalidOperationException("Inactive customers cannot renew subscriptions");
            }


            decimal baseAmount = (plan.MonthlyPricePerSeat * seatCount * 12m) + plan.SetupFee;
            string finalNotes = string.Empty;


            decimal discountAmount = 0m;
            foreach (var rule in _discountRules)
            {
                var ruleResult = rule.Calculate(customer, plan, seatCount, baseAmount, useLoyaltyPoints);
                discountAmount += ruleResult.DiscountAmount;
                finalNotes += ruleResult.Note;
            }

            decimal subtotalAfterDiscount = baseAmount - discountAmount;
            if (subtotalAfterDiscount < 300m)
            {
                subtotalAfterDiscount = 300m;
                discountAmount = baseAmount - subtotalAfterDiscount; 
                finalNotes += "minimum discounted subtotal applied; ";
            }


            decimal supportFee = _feeCalculator.CalculateSupportFee(normalizedPlanCode, includePremiumSupport, out string supportNote);
            finalNotes += supportNote;

            decimal paymentFee = _feeCalculator.CalculatePaymentFee(normalizedPaymentMethod, subtotalAfterDiscount + supportFee, out string paymentNote);
            finalNotes += paymentNote;


            decimal taxBase = subtotalAfterDiscount + supportFee + paymentFee;
            decimal taxAmount = _taxCalculator.CalculateTax(customer.Country, taxBase);
            decimal finalAmount = taxBase + taxAmount;

            if (finalAmount < 500m)
            {
                finalAmount = 500m;
                finalNotes += "minimum invoice amount applied; ";
            }


            var invoice = CreateInvoiceObject(customerId, normalizedPlanCode, normalizedPaymentMethod, seatCount, baseAmount, discountAmount, supportFee, paymentFee, taxAmount, finalAmount, finalNotes, customer.FullName);

            _billingGateway.SaveInvoice(invoice);
            SendEmailNotification(customer, normalizedPlanCode, invoice.FinalAmount);

            return invoice;
        }



        private void ValidateInputs(int customerId, string planCode, int seatCount, string paymentMethod)
        {
            if (customerId <= 0) throw new ArgumentException("Customer id must be positive");
            if (string.IsNullOrWhiteSpace(planCode)) throw new ArgumentException("Plan code is required");
            if (seatCount <= 0) throw new ArgumentException("Seat count must be positive");
            if (string.IsNullOrWhiteSpace(paymentMethod)) throw new ArgumentException("Payment method is required");
        }

        private RenewalInvoice CreateInvoiceObject(int customerId, string planCode, string paymentMethod, int seatCount, decimal baseAmount, decimal discountAmount, decimal supportFee, decimal paymentFee, decimal taxAmount, decimal finalAmount, string notes, string customerName)
        {
            return new RenewalInvoice
            {
                InvoiceNumber = $"INV-{DateTime.UtcNow:yyyyMMdd}-{customerId}-{planCode}",
                CustomerName = customerName,
                PlanCode = planCode,
                PaymentMethod = paymentMethod,
                SeatCount = seatCount,
                BaseAmount = Math.Round(baseAmount, 2, MidpointRounding.AwayFromZero),
                DiscountAmount = Math.Round(discountAmount, 2, MidpointRounding.AwayFromZero),
                SupportFee = Math.Round(supportFee, 2, MidpointRounding.AwayFromZero),
                PaymentFee = Math.Round(paymentFee, 2, MidpointRounding.AwayFromZero),
                TaxAmount = Math.Round(taxAmount, 2, MidpointRounding.AwayFromZero),
                FinalAmount = Math.Round(finalAmount, 2, MidpointRounding.AwayFromZero),
                Notes = notes.Trim(),
                GeneratedAt = DateTime.UtcNow
            };
        }

        private void SendEmailNotification(Customer customer, string planCode, decimal finalAmount)
        {
            if (!string.IsNullOrWhiteSpace(customer.Email))
            {
                string subject = "Subscription renewal invoice";
                string body = $"Hello {customer.FullName}, your renewal for plan {planCode} has been prepared. Final amount: {finalAmount:F2}.";
                
                _billingGateway.SendEmail(customer.Email, subject, body);
            }
        }
    }
}