namespace LegacyRenewalApp.Interfaces;

public interface IPaymentFeeCalculator
{
    (decimal Fee, string Note) Calculate(string normalizedPaymentMethod, decimal subtotalWithSupport);
}