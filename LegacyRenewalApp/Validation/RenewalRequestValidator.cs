using System;

namespace LegacyRenewalApp.Validation;

public static class RenewalRequestValidator
{
    public static void Validate(int customerId, string planCode, int seatCount, string paymentMethod)
    {
        if (customerId <= 0)
            throw new ArgumentException("CustomerId must be positive");
        if (string.IsNullOrWhiteSpace(planCode))
            throw new ArgumentException("Plan code is required");
        if (seatCount <= 0)
            throw new ArgumentException("SeatCount must be positive");
        if (string.IsNullOrWhiteSpace(paymentMethod))
            throw new ArgumentException("Payment method is required");
    }
}