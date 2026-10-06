namespace EClaim.Application.Common;

public static class ClaimNumberGenerator
{
    public static string Generate(int sequentialId) => $"CLM-{DateTime.UtcNow:yyyy}-{sequentialId:D5}";
}