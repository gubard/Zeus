using Microsoft.Extensions.Logging;

namespace Zeus.Helpers;

public static partial class ZeusLogs
{
    [LoggerMessage(
        EventId = 1,
        Level = LogLevel.Information,
        Message = "Delete idempotent entities: {IdempotentIds}. For user {UserId}"
    )]
    public static partial void DeleteIdempotentEntities(
        this ILogger logger,
        Guid userId,
        Guid[] idempotentIds
    );
}
