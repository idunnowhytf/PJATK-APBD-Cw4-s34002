namespace LegacyRenewalApp
{

    public interface ICustomerRepository
    {
        Customer GetById(int customerId);
    }

    public interface ISubscriptionPlanRepository
    {
        SubscriptionPlan GetByCode(string code);
    }


    public interface IBillingGateway
    {
        void SaveInvoice(RenewalInvoice invoice);
        void SendEmail(string to, string subject, string body);
    }
    
    public class DiscountResult
    {
        public decimal DiscountAmount { get; set; }
        public string Note { get; set; } = string.Empty;
    }
    
    public interface IDiscountRule
    {
        DiscountResult Calculate(Customer customer, SubscriptionPlan plan, int seatCount, decimal baseAmount, bool useLoyaltyPoints);
    }
    
    public interface ITaxCalculator
    {
        decimal CalculateTax(string country, decimal taxBase);
    }

    public interface IFeeCalculator
    {
        decimal CalculateSupportFee(string planCode, bool includePremiumSupport, out string note);
        decimal CalculatePaymentFee(string paymentMethod, decimal amountToCharge, out string note);
    }
}