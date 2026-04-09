namespace LegacyRenewalApp
{
    public class BillingGatewayAdapter : IBillingGateway
    {
        public void SaveInvoice(RenewalInvoice invoice)
        {
            LegacyBillingGateway.SaveInvoice(invoice);
        }

        public void SendEmail(string to, string subject, string body)
        {
            LegacyBillingGateway.SendEmail(to, subject, body);
        }
    }
}