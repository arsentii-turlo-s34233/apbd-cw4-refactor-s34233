using LegacyRenewalApp.Interfaces;
using System;

namespace LegacyRenewalApp.Discounts;

public class LoyaltyPointsDiscountStrategy : IDiscountStrategy
{
    public (decimal Amount, string Note) Calculate(Customer customer, SubscriptionPlan plan,
        int seatCount, decimal baseAmount, bool useLoyaltyPoints)
    {
        if (!useLoyaltyPoints || customer.LoyaltyPoints <= 0)
            return (0m, string.Empty);

        int pointsToUse = Math.Min(customer.LoyaltyPoints, 200);
        return (pointsToUse, $"loyalty points used: {pointsToUse}");
    }
}