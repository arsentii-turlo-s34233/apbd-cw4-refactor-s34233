namespace LegacyRenewalApp.Interfaces;

public interface ITaxRateProvider
{
    decimal GetRate(string country);
}