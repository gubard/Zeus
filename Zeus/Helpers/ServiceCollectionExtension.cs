using Gaia.Helpers;
using Gaia.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Nestor.Db.Sqlite;

namespace Zeus.Helpers;

public static class ServiceCollectionExtension
{
    extension(IServiceCollection serviceCollection)
    {
        public IServiceCollection AddJwtAuthentication(IConfiguration configuration)
        {
            serviceCollection.AddAuthentication(x => x.SetJwtBearerDefaults())
               .AddJwtBearer(x => x.SetJwtOptions(configuration));

            return serviceCollection;
        }

        public IServiceCollection AddZeusDbContext(string name)
        {
            return serviceCollection.AddDbContext<DbContext, SqliteNestorDbContext>((sp, options) =>
            {
                var httpContextAccessor = sp.GetRequiredService<IHttpContextAccessor>();
                var userId = httpContextAccessor.HttpContext.ThrowIfNull().GetUserId();
                var dataSourceFile = sp.GetRequiredService<IStorageService>().GetDbDirectory().Combine(name).ToFile($"{userId}.db");
                options.UseSqlite($"Data Source={dataSourceFile}", x => x.MigrationsAssembly(typeof(SqliteNestorDbContext).Assembly));

                if (dataSourceFile.Exists)
                {
                    return;
                }

                if (dataSourceFile.Directory?.Exists != true)
                {
                    dataSourceFile.Directory?.Create();
                }

                using var context = new SqliteNestorDbContext(options.Options);
                context.Database.Migrate();
            });
        }
    }

}