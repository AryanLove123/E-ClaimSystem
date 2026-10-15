using EClaim.Domain.Common;
using EClaim.Domain.Enums;

namespace EClaim.Domain.Entities;

public class ErrorLog : BaseEntity
{
    public EClaimLogLevel Level { get; set; } = EClaimLogLevel.Error;
    public string Message { get; set; } = string.Empty;
    public string? ExceptionType { get; set; }
    public string? StackTrace { get; set; }
    public string? RequestPath { get; set; }
    public int? UserId { get; set; }
    public string TraceId { get; set; } = string.Empty;
}
