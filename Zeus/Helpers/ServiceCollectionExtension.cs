using Gaia.Helpers;
using Gaia.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Nestor.Db.Models;
using Nestor.Db.Services;

namespace Zeus.Helpers;

public static class ServiceCollectionExtension
{
    extension(IServiceCollection serviceCollection)
    {
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
