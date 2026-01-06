using System.Runtime.CompilerServices;
using Gaia.Services;
using Microsoft.EntityFrameworkCore;
using Nestor.Db.Services;
using Nestor.Db.Sqlite.Services;

namespace Zeus.Services;

public interface IZeusMigrator
{
    ConfiguredValueTaskAwaitable MigrateAsync(CancellationToken ct);
}

public class ZeusMigrator<TFactory> : IZeusMigrator
    where TFactory : IStaticFactory<DbContextOptions, NestorDbContext>
{
    private readonly DirectoryInfo _dbsDirectory;
    private readonly IMigrator _migrator;

    public ZeusMigrator(DirectoryInfo dbsDirectory, IMigrator migrator)
    {
        _dbsDirectory = dbsDirectory;
        _migrator = migrator;
    }

    public ConfiguredValueTaskAwaitable MigrateAsync(CancellationToken ct)
    {
        return MigrateCore(ct).ConfigureAwait(false);
    }

    private async ValueTask MigrateCore(CancellationToken ct)
    {
        if (!_dbsDirectory.Exists)
        {
            _dbsDirectory.Create();
        }

        var files = _dbsDirectory.GetFiles("*.db");

        foreach (var file in files)
        {
            var options = new DbContextOptionsBuilder().UseSqlite($"Data Source={file}").Options;
            await using var context = TFactory.Create(options);
            await _migrator.MigrateAsync(context, ct);
        }
    }
}
