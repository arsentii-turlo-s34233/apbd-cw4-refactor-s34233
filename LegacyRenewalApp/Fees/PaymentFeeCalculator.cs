using LegacyRenewalApp.Interfaces;
using System;

namespace LegacyRenewalApp.Fees;

public class PaymentFeeCalculator : IPaymentFeeCalculator
{
    public (decimal Fee, string Note) Calculate(string normalizedPaymentMethod, decimal subtotalWithSupport)
    {
        return normalizedPaymentMethod switch
        {
            "CARD"          => (subtotalWithSupport * 0.02m,  "card payment fee"),
            "BANK_TRANSFER" => (subtotalWithSupport * 0.01m,  "bank transfer fee"),
            "PAYPAL"        => (subtotalWithSupport * 0.035m, "paypal fee"),
            "INVOICE"       => (0m,                           "invoice payment"),
            _               => throw new ArgumentException("Unsupported payment method")
        };
    }
}