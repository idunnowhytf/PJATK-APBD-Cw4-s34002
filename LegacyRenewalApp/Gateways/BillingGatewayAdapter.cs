namespace LegacyRenewalApp
{
    /// <summary>
    /// Adapter komunikujący kod kliencki (Domain) ze statyczną bramką sprzętową (LegacyBillingGateway).
    /// </summary>
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