using Gaia.Helpers;
using Microsoft.Extensions.Hosting;
using Nestor.Db.Helpers;
using Nestor.Db.Models;
using Nestor.Db.Services;

namespace Zeus.Services;

public sealed class IdempotenceCleanerBackgroundService : BackgroundService
{
    public IdempotenceCleanerBackgroundService(DirectoryInfo dbsDirectory)
    {
        _dbsDirectory = dbsDirectory;
    }

    protected override async Task ExecuteAsync(CancellationToken ct)
    {
        while (!ct.IsCancellationRequested)
        {
            if (!_dbsDirectory.Exists)
            {
                return;
            }

            var files = _dbsDirectory.GetFiles("*.db");

            foreach (var file in files)
            {
                var factory = new SqliteDbConnectionFactory(file);
                await using var session = await factory.CreateSessionAsync(ct);
                var query = IdempotentsExt.SelectQuery;
                var items = await ReadIdempotentsAsync(session, query, ct);

                var deleteIds = items
                    .Where(x => DateTimeOffset.UtcNow - x.CreatedAt >= Offset)
                    .Select(x => x.Id)
                    .ToArray();

                if (deleteIds.Length == 0)
                {
                    continue;
                }

                await session.ExecuteNonQueryAsync(deleteIds.CreateDeleteIdempotentsQuery(), ct);
                await session.CommitAsync(ct);
            }

            await Task.Delay(Offset, ct);
        }
    }

    private static readonly TimeSpan Offset = TimeSpan.FromDays(1);

    private readonly DirectoryInfo _dbsDirectory;

    private async ValueTask<IEnumerable<IdempotentEntity>> ReadIdempotentsAsync(
        DbSession session,
        SqlQuery query,
        CancellationToken ct
    )
    {
        await using var reader = await session.ExecuteReaderAsync(query, ct);
        var items = await reader.ReadIdempotentsAsync(ct).ToEnumerableAsync();

        return items;
    }
}
