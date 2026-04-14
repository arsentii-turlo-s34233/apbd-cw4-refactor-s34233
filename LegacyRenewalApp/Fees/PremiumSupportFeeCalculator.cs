namespace LegacyRenewalApp.Fees;

public class PremiumSupportFeeCalculator
{
    public static decimal Calculate(bool includePremiumSupport, string normalizedPlanCode)
    {
        if (!includePremiumSupport)
            return 0m;

        return normalizedPlanCode switch
        {
            "START"      => 250m,
            "PRO"        => 400m,
            "ENTERPRISE" => 700m,
            _            => 0m
        };
    }
}