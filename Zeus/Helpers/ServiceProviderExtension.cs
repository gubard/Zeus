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
            return serviceProvider.GetRequiredService<IConfiguration>().GetSection(path).Get<T>().ThrowIfNull();
        }

        public void CreateDbDirectory()
        {
            var storage = serviceProvider.GetRequiredService<IStorageService>();
            var dbDirectory = storage.GetDbDirectory();

            if (dbDirectory.Exists)
            {
                return;
            }

            dbDirectory.Create();
        }

        public async Task MigrateDbAsync()
        {
            using var scope = serviceProvider.CreateScope();
            await using var context = scope.ServiceProvider.GetRequiredService<DbContext>();
            await context.Database.MigrateAsync();
        }
    }
}