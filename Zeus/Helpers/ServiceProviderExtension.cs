using Gaia.Helpers;
using Gaia.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using IServiceProvider = System.IServiceProvider;

namespace Zeus.Helpers;

public static class ServiceProviderExtension
{
    extension(IServiceProvider serviceProvider)
    {
        public T GetConfigurationSection<T>(string path)
        {
            return serviceProvider
                .GetRequiredService<IConfiguration>()
                .GetConfigurationSection<T>(path);
        }

        public void CreateDbDirectory()
        {
            var storageService = serviceProvider.GetRequiredService<IStorageService>();
            var dbDirectory = storageService.GetDbDirectory();

            if (dbDirectory.Exists)
            {
                return;
            }

            dbDirectory.Create();
        }

        public async Task MigrateDbAsync(string migrateFileName, CancellationToken ct)
        {
            var storageService = serviceProvider.GetRequiredService<IStorageService>();
            var migrationFile = storageService.GetDbDirectory().ToFile(migrateFileName);
            using var scope = serviceProvider.CreateScope();
            await using var context = scope.ServiceProvider.GetRequiredService<DbContext>();
            var migrationName = context.Database.GetMigrations().Last();

            if (!migrationFile.Exists)
            {
                await context.Database.MigrateAsync(ct);
                await migrationFile.WriteAllTextAsync(migrationName, ct);
            }

            var currentMigration = await migrationFile.ReadAllTextAsync(ct);

            if (currentMigration == migrationName)
            {
                return;
            }

            await context.Database.MigrateAsync(ct);
            await migrationFile.WriteAllTextAsync(migrationName, ct);
        }
    }
}
