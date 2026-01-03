using Microsoft.EntityFrameworkCore;
using Nestor.Db.Services;
using Nestor.Db.Sqlite;

namespace Zeus.Services;

public interface IZeusMigrator
{
    ValueTask MigrateAsync(CancellationToken ct);
}

public class ZeusMigrator : IZeusMigrator
{
    private readonly DirectoryInfo _dbsDirectory;
    private readonly IMigrator _migrator;

    public ZeusMigrator(DirectoryInfo dbsDirectory, IMigrator migrator)
    {
        _dbsDirectory = dbsDirectory;
        _migrator = migrator;
    }

    public async ValueTask MigrateAsync(CancellationToken ct)
    {
        if (!_dbsDirectory.Exists)
        {
            _dbsDirectory.Create();
        }

        var files = _dbsDirectory.GetFiles("*.db");

        foreach (var file in files)
        {
            var options = new DbContextOptionsBuilder().UseSqlite($"Data Source={file}").Options;
            await using var context = new SqliteNestorDbContext(options);
            await _migrator.MigrateAsync(context, ct);
        }
    }
}
