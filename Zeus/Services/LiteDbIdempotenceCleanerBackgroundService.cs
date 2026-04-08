using Gaia.Helpers;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Nestor.Db.Models;
using UltraLiteDB;
using Zeus.Helpers;

namespace Zeus.Services;

public sealed class LiteDbIdempotenceCleanerBackgroundService : BackgroundService
{
    public LiteDbIdempotenceCleanerBackgroundService(
        DirectoryInfo dbsDirectory,
        GuidDatabaseFactory factory,
        ILogger<LiteDbIdempotenceCleanerBackgroundService> logger
    )
    {
        _dbsDirectory = dbsDirectory;
        _factory = factory;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken ct)
    {
        try
        {
            while (!ct.IsCancellationRequested)
            {
                if (!_dbsDirectory.Exists)
                {
                    return;
                }

                var files = _dbsDirectory.GetFiles("*.litedb");

                foreach (var file in files)
                {
                    if (!Guid.TryParse(file.GetFileNameWithoutExtension(), out var id))
                    {
                        continue;
                    }

                    var database = _factory.Create(id);

                    await database.ExecuteAsync(
                        db =>
                        {
                            var collection = db.GetIdempotentEntityCollection();

                            var deleteIds = collection
                                .FindAll()
                                .Select(x => x.ToIdempotentEntity())
                                .Where(x => DateTimeOffset.UtcNow - x.CreatedAt >= Offset)
                                .Select(x => x.Id)
                                .ToArray();

                            if (deleteIds.Length == 0)
                            {
                                return;
                            }

                            collection.Delete(
                                Query.In("_id", deleteIds.Select(x => new BsonValue(x)))
                            );
                            _logger.DeleteIdempotentEntities(id, deleteIds);
                        },
                        ct
                    );
                }

                await Task.Delay(Offset, ct);
            }
        }
        catch (OperationCanceledException) { }
        catch (Exception e)
        {
            _logger.LogError(e, $"{nameof(LiteDbIdempotenceCleanerBackgroundService)} error");
        }
    }

    private static readonly TimeSpan Offset = TimeSpan.FromDays(1);

    private readonly DirectoryInfo _dbsDirectory;
    private readonly GuidDatabaseFactory _factory;
    private readonly ILogger<LiteDbIdempotenceCleanerBackgroundService> _logger;
}
