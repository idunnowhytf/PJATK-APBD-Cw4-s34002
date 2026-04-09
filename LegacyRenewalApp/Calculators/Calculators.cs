using System;

namespace LegacyRenewalApp
{
    public class TaxCalculator : ITaxCalculator
    {
        public decimal CalculateTax(string country, decimal taxBase)
        {
            decimal taxRate = 0.20m; 

            switch (country)
            {
                case "Poland": taxRate = 0.23m; break;
                case "Germany": taxRate = 0.19m; break;
                case "Czech Republic": taxRate = 0.21m; break;
                case "Norway": taxRate = 0.25m; break;
            }

            return taxBase * taxRate;
        }
    }

    public class FeeCalculator : IFeeCalculator
    {
        public decimal CalculateSupportFee(string planCode, bool includePremiumSupport, out string note)
        {
            note = string.Empty;
            if (!includePremiumSupport) return 0m;

            decimal supportFee = 0m;
            if (planCode == "START") supportFee = 250m;
            else if (planCode == "PRO") supportFee = 400m;
            else if (planCode == "ENTERPRISE") supportFee = 700m;

            note = "premium support included; ";
            return supportFee;
        }

        public decimal CalculatePaymentFee(string paymentMethod, decimal amountToCharge, out string note)
        {
            decimal paymentFee = 0m;
            note = string.Empty;

            if (paymentMethod == "CARD")
            {
                paymentFee = amountToCharge * 0.02m;
                note = "card payment fee; ";
            }
            else if (paymentMethod == "BANK_TRANSFER")
            {
                paymentFee = amountToCharge * 0.01m;
                note = "bank transfer fee; ";
            }
            else if (paymentMethod == "PAYPAL")
            {
                paymentFee = amountToCharge * 0.035m;
                note = "paypal fee; ";
            }
            else if (paymentMethod == "INVOICE")
            {
                paymentFee = 0m;
                note = "invoice payment; ";
            }
            else
            {
                throw new ArgumentException("Unsupported payment method");
            }

            return paymentFee;
        }
    }
}