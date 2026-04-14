using LegacyRenewalApp.Interfaces;

namespace LegacyRenewalApp.Infrastructure;

public class LegacyBillingGatewayAdapter : IBillingGateway
{
    public void SendInvoice(RenewalInvoice invoice)
    {
        LegacyBillingGateway.SaveInvoice(invoice);
    }

    public void SendEmail(string email, string subject, string body)
    {
        LegacyBillingGateway.SendEmail(email, subject, body);
    }
}