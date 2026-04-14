namespace LegacyRenewalApp.Interfaces;

public interface IBillingGateway
{
    void SendInvoice(RenewalInvoice invoice);
    void SendEmail(string email, string subject, string body);
}