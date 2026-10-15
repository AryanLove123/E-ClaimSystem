using EClaim.Domain.Entities;
using EClaim.Domain.Enums;
using EClaim.Infrastructure.Persistence;

namespace EClaim.Infrastructure.Logging;

public class DatabaseLoggerService
{
    private EClaimDbContext _dbContext;

    public DatabaseLoggerService(EClaimDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task LogErrorAsync(Exception exception, string traceId, string? requestPath, int? userId)
    {
        _dbContext.ErrorLogs.Add(new ErrorLog
        {
            Level = EClaimLogLevel.Error,
            Message = exception.Message,
            ExceptionType = exception.GetType().FullName,
            StackTrace = exception.StackTrace,
            RequestPath = requestPath,
            UserId = userId,
            TraceId = traceId
        });
        await _dbContext.SaveChangesAsync();
    }
}