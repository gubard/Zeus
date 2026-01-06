using System.Collections.Frozen;
using Gaia.Helpers;
using Gaia.Models;
using Gaia.Services;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Nestor.Db.Services;
using Nestor.Db.Sqlite.Services;
using Zeus.Services;

namespace Zeus.Helpers;

public static class WebApplicationBuilderExtension
{
    extension(WebApplicationBuilder builder)
    {
        public ValueTask CreateAndRunZeusApp<
            TServiceInterface,
            TService,
            TGetRequest,
            TPostRequest,
            TGetResponse,
            TPostResponse,
            TDbContext
        >(FrozenDictionary<int, string> migrations, string name)
            where TServiceInterface : class,
                IService<TGetRequest, TPostRequest, TGetResponse, TPostResponse>
            where TService : class, TServiceInterface
            where TGetResponse : IValidationErrors, new()
            where TPostResponse : IValidationErrors, new()
            where TDbContext : NestorDbContext, IStaticFactory<DbContextOptions, NestorDbContext>
        {
            builder.AddServicesZeus<
                TServiceInterface,
                TService,
                TGetRequest,
                TPostRequest,
                TGetResponse,
                TPostResponse,
                TDbContext
            >(migrations, name);

            var app = builder.Build();

            return app.RunZeusApp<
                TServiceInterface,
                TGetRequest,
                TPostRequest,
                TGetResponse,
                TPostResponse
            >();
        }

        public WebApplicationBuilder AddServicesZeus<
            TServiceInterface,
            TService,
            TGetRequest,
            TPostRequest,
            TGetResponse,
            TPostResponse,
            TDbContext
        >(FrozenDictionary<int, string> migrations, string name)
            where TServiceInterface : class,
                IService<TGetRequest, TPostRequest, TGetResponse, TPostResponse>
            where TService : class, TServiceInterface
            where TGetResponse : IValidationErrors, new()
            where TPostResponse : IValidationErrors, new()
            where TDbContext : NestorDbContext, IStaticFactory<DbContextOptions, NestorDbContext>
        {
            builder.Services.AddOpenApi();
            builder.Services.AddAuthorization();
            builder.Services.AddHttpContextAccessor();
            builder.Services.AddScoped<GaiaValues>(sp =>
                sp.GetRequiredService<IHttpContextAccessor>()
                    .HttpContext.ThrowIfNull()
                    .GetRequestValues()
            );
            builder.Services.AddJwtAuthentication(builder.Configuration);
            builder.Services.AddTransient<IStorageService>(_ => new StorageService("Zeus"));
            builder.Services.AddTransient<TServiceInterface, TService>();
            builder.Services.AddTransient<IMigrator>(_ => new Migrator(migrations));
            builder.Services.AddTransient<IZeusMigrator, ZeusMigrator<TDbContext>>(sp =>
                new(
                    sp.GetRequiredService<IStorageService>().GetDbDirectory().Combine(name),
                    sp.GetRequiredService<IMigrator>()
                )
            );
            builder.Services.AddZeusDbContext<TDbContext>(name);

            return builder;
        }
    }
}
