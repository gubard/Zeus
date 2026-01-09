using Gaia.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Nestor.Db.Models;
using Nestor.Db.Services;
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

        public async Task MigrateDbAsync(CancellationToken ct)
        {
            var migrator = serviceProvider.GetRequiredService<IMigrator>();
            using var scope = serviceProvider.CreateScope();
            var factory = scope.ServiceProvider.GetRequiredService<IDbConnectionFactory>();
            await migrator.MigrateAsync(factory, ct);
        }
    }
}
