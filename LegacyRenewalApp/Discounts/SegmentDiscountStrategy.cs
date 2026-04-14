using LegacyRenewalApp.Interfaces;

namespace LegacyRenewalApp.Discounts;

public class SegmentDiscountStrategy : IDiscountStrategy
{
    public (decimal Amount, string Note) Calculate(Customer customer, SubscriptionPlan plan, int seatCount, decimal baseAmount,
        bool useLoyaltyPoints)
    {
        return customer.Segment switch
        {
            "Silver" => (baseAmount * 0.05m, "silver discount"),
            "Gold" => (baseAmount * 0.10m, "gold discount"),
            "Platinum" => (baseAmount * 0.15m, "platinum discount"),
            "Education" when plan.IsEducationEligible => (baseAmount * 0.20m, "education discount"),
            _ => (0m, string.Empty)
        };
    }
}