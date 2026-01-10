using System.Collections.Frozen;
using Gaia.Helpers;
using Gaia.Models;
using Gaia.Services;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Nestor.Db.Models;
using Nestor.Db.Services;
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
            TPostResponse
        >(FrozenDictionary<int, string> migrations, string name)
            where TServiceInterface : class,
                IService<TGetRequest, TPostRequest, TGetResponse, TPostResponse>
            where TService : class, TServiceInterface
            where TGetResponse : IValidationErrors, new()
            where TPostResponse : IValidationErrors, new()
        {
            builder.AddServicesZeus<
                TServiceInterface,
                TService,
                TGetRequest,
                TPostRequest,
                TGetResponse,
                TPostResponse
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
            TPostResponse
        >(FrozenDictionary<int, string> migrations, string name)
            where TServiceInterface : class,
                IService<TGetRequest, TPostRequest, TGetResponse, TPostResponse>
            where TService : class, TServiceInterface
            where TGetResponse : IValidationErrors, new()
            where TPostResponse : IValidationErrors, new()
        {
            builder.Services.AddOpenApi();
            builder.Services.AddAuthorization();
            builder.Services.AddHttpContextAccessor();
            builder.Services.AddTransient<IStorageService>(_ => new StorageService("Zeus"));
            builder.Services.AddTransient<TServiceInterface, TService>();
            builder.Services.AddTransient<IMigrator>(_ => new Migrator(migrations));
            builder.Services.AddJwtAuthentication(builder.Configuration);
            builder.Services.AddZeusDb(name);

            builder.Services.AddScoped<GaiaValues>(sp =>
                sp.GetRequiredService<IHttpContextAccessor>()
                    .HttpContext.ThrowIfNull()
                    .GetRequestValues()
            );

            builder.Services.AddSingleton<IFactory<DbServiceOptions>>(
                _ => new DbServiceOptionsFactory(new(true))
            );

            builder.Services.AddTransient<IZeusMigrator, ZeusMigrator>(sp =>
                new(
                    sp.GetRequiredService<IStorageService>().GetDbDirectory().Combine(name),
                    sp.GetRequiredService<IMigrator>()
                )
            );

            return builder;
        }
    }
}
