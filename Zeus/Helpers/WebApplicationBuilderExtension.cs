using System.Collections.Frozen;
using System.Text.Json;
using Gaia.Helpers;
using Gaia.Models;
using Gaia.Services;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Nestor.Db.LiteDb.Services;
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
        >(
            FrozenDictionary<int, string> migrations,
            string name,
            Action<WebApplicationBuilder> configure
        )
            where TServiceInterface : class,
                IService<TGetRequest, TPostRequest, TGetResponse, TPostResponse>
            where TService : class, TServiceInterface
            where TGetResponse : IValidationErrors, new()
            where TPostResponse : class, IValidationErrors, new()
        {
            builder.AddServicesZeus<
                TServiceInterface,
                TService,
                TGetRequest,
                TPostRequest,
                TGetResponse,
                TPostResponse
            >(migrations, name);

            configure.Invoke(builder);

            var app = builder.Build();

            return app.RunZeusApp<
                TServiceInterface,
                TGetRequest,
                TPostRequest,
                TGetResponse,
                TPostResponse
            >();
        }

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
            where TPostResponse : class, IValidationErrors, new()
        {
            return CreateAndRunZeusApp<
                TServiceInterface,
                TService,
                TGetRequest,
                TPostRequest,
                TGetResponse,
                TPostResponse
            >(builder, migrations, name, FuncHelper<WebApplicationBuilder>.Empty);
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
            builder.WebHost.ConfigureKestrel(options =>
                options.Limits.MaxRequestBodySize = 10 * 1024 * 1024
            );

            builder.Services.AddCors(o => o.AddAllowAllPolicy());
            builder.Services.AddOpenApi();
            builder.Services.AddAuthorization();
            builder.Services.AddHttpContextAccessor();

            builder.Services.AddSingleton<IStorageService>(sp => new StorageService(
                "Zeus",
                sp.GetRequiredService<ILogger<StorageService>>()
            ));

            builder.Services.AddTransient<TServiceInterface, TService>();

            builder.Services.AddSingleton<GuidDatabaseFactory>(sp =>
                new(sp.GetRequiredService<IStorageService>(), name)
            );

            builder.Services.AddTransient<IMigrator>(_ => new Migrator(migrations));
            builder.Services.AddJwtAuthentication(builder.Configuration);
            builder.Services.AddZeusDb(name);
            builder.Services.AddIdempotence(name);
            builder.Services.AddScoped<IFactory<DbValues>, DbValuesFactory>();

            builder.Services.AddScoped<IDatabaseFactory>(sp => new ValueDatabaseFactory(
                sp.GetRequiredService<GuidDatabaseFactory>()
                    .Create(sp.GetRequiredService<IFactory<DbValues>>().Create().UserId)
            ));

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
