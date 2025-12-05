using System.Reflection;
using Gaia.Helpers;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Nestor.Db.Sqlite;

namespace Zeus.Services;

public interface IDbMigrator
{
    ValueTask MigrateAsync(CancellationToken ct);
}

public class DbMigrator : IDbMigrator
{
    private readonly DirectoryInfo _dbsDirectory;

    public DbMigrator(DirectoryInfo dbsDirectory)
    {
        _dbsDirectory = dbsDirectory;
    }

    public async ValueTask MigrateAsync(CancellationToken ct)
    {
        if (!_dbsDirectory.Exists)
        {
            _dbsDirectory.Create();
        }

        var migrationId = GetMigrationId();
        var migrationFile = _dbsDirectory.ToFile(".migration");

        if (!migrationFile.Exists)
        {
            await using var stream = migrationFile.Create();
        }

        if (migrationId == await migrationFile.ReadAllTextAsync(ct))
        {
            return;
        }

        var files = _dbsDirectory.GetFiles("*.db");

        foreach (var file in files)
        {
            var options = new DbContextOptionsBuilder().UseSqlite($"Data Source={file}", x => x.MigrationsAssembly(typeof(SqliteNestorDbContext).Assembly)).Options;
            await using var context = new SqliteNestorDbContext(options);
            await context.Database.MigrateAsync(ct);
        }

        await migrationFile.WriteAllTextAsync(migrationId, ct);
    }

    private string GetMigrationId()
    {
        return AppDomain.CurrentDomain
           .GetAssemblies()
           .SelectMany(x => x.GetTypes())
           .Where(
                x =>
                {
                    var dbContextAttribute = x.GetCustomAttribute<DbContextAttribute>();

                    if (dbContextAttribute is null)
                    {
                        return false;
                    }

                    return dbContextAttribute.ContextType == typeof(SqliteNestorDbContext);
                }
            )
           .Select(x => x.GetCustomAttribute<MigrationAttribute>())
           .Where(x => x is not null)
           .Select(x => x.ThrowIfNull().Id)
           .OrderByDescending(x => x)
           .First();
    }
}