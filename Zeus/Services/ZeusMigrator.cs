using System.Runtime.CompilerServices;
using Nestor.Db.Models;
using Nestor.Db.Services;

namespace Zeus.Services;

public interface IZeusMigrator
{
    ConfiguredValueTaskAwaitable MigrateAsync(CancellationToken ct);
}

public sealed class ZeusMigrator : IZeusMigrator
{
    public ZeusMigrator(DirectoryInfo dbsDirectory, IMigrator migrator)
    {
        _dbsDirectory = dbsDirectory;
        _migrator = migrator;
    }

    public ConfiguredValueTaskAwaitable MigrateAsync(CancellationToken ct)
    {
        return MigrateCore(ct).ConfigureAwait(false);
    }

    private readonly DirectoryInfo _dbsDirectory;
    private readonly IMigrator _migrator;

    private async ValueTask MigrateCore(CancellationToken ct)
    {
        if (!_dbsDirectory.Exists)
        {
            _dbsDirectory.Create();
        }

        var files = _dbsDirectory.GetFiles("*.db");

        foreach (var file in files)
        {
            var factory = new SqliteDbConnectionFactory(file);
            await _migrator.MigrateAsync(factory, ct);
        }
    }
}
