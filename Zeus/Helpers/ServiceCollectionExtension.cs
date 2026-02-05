using System.Text.Json;
using Gaia.Helpers;
using Gaia.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Nestor.Db.Models;
using Nestor.Db.Services;
using Zeus.Services;
using JsonSerializer = Gaia.Services.JsonSerializer;

namespace Zeus.Helpers;

public static class ServiceCollectionExtension
{
    extension(IServiceCollection serviceCollection)
    {
        public IServiceCollection AddIdempotence(JsonSerializerOptions options, string name)
        {
            serviceCollection.AddSingleton(options);
            serviceCollection.AddTransient<ISerializer, JsonSerializer>();
            serviceCollection.AddScoped<IIdempotenceService, IdempotenceService>();

            serviceCollection.AddHostedService(sp => new IdempotenceCleanerBackgroundService(
                sp.GetRequiredService<IStorageService>().GetDbDirectory().Combine(name)
            ));

            return serviceCollection;
        }

        public IServiceCollection AddJwtAuthentication(IConfiguration configuration)
        {
            serviceCollection
                .AddAuthentication(x => x.SetJwtBearerDefaults())
                .AddJwtBearer(x => x.SetJwtOptions(configuration));

            return serviceCollection;
        }

        public IServiceCollection AddZeusDb(string name)
        {
            return serviceCollection.AddScoped<IDbConnectionFactory>(sp =>
            {
                var httpContextAccessor = sp.GetRequiredService<IHttpContextAccessor>();
                var userId = httpContextAccessor.HttpContext.ThrowIfNull().GetUserId();

                var dataSourceFile = sp.GetRequiredService<IStorageService>()
                    .GetDbDirectory()
                    .Combine(name)
                    .ToFile($"{userId}.db");

                var factory = new SqliteDbConnectionFactory(dataSourceFile);

                if (dataSourceFile.Exists)
                {
                    return factory;
                }

                if (dataSourceFile.Directory?.Exists != true)
                {
                    dataSourceFile.Directory?.Create();
                }

                sp.GetRequiredService<IMigrator>().Migrate(factory);

                return factory;
            });
        }
    }
}
