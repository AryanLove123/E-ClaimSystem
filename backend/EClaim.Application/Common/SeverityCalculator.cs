using EClaim.Domain.Enums;

namespace EClaim.Application.Common;

public static class SeverityCalculator
{
    public const decimal MediumThreshold = 50_000m;
    public const decimal HighThreshold = 200_000m;
    public const decimal CriticalThreshold = 500_000m;

    public static ClaimSeverity Calculate(decimal amount)
    {
        if (amount > CriticalThreshold) return ClaimSeverity.Critical;
        if (amount > HighThreshold) return ClaimSeverity.High;
        if (amount > MediumThreshold) return ClaimSeverity.Medium;
        return ClaimSeverity.Low;
    }
}