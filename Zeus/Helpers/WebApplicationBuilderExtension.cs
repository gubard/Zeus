using Gaia.Helpers;
using Gaia.Models;
using Gaia.Services;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Zeus.Services;

namespace Zeus.Helpers;

public static class WebApplicationBuilderExtension
{
    extension(WebApplicationBuilder builder)
    {
        public ValueTask
            CreateAndRunZeusApp<TServiceInterface, TService, TGetRequest,
                TPostRequest,
                TGetResponse, TPostResponse>(string name)
            where TServiceInterface : class, IService<TGetRequest, TPostRequest,
                TGetResponse, TPostResponse>
            where TService : class, TServiceInterface
            where TGetResponse : IValidationErrors, new()
            where TPostResponse : IValidationErrors, new()
        {
            builder
               .AddServicesZeus<TServiceInterface, TService, TGetRequest,
                    TPostRequest, TGetResponse, TPostResponse>(name);

            var app = builder.Build();

            return app.RunZeusApp<TServiceInterface,
                TGetRequest,
                TPostRequest, TGetResponse, TPostResponse>();
        }

        public WebApplicationBuilder
            AddServicesZeus<TServiceInterface, TService, TGetRequest,
                TPostRequest,
                TGetResponse, TPostResponse>(string name)
            where TServiceInterface : class, IService<TGetRequest, TPostRequest,
                TGetResponse, TPostResponse>
            where TService : class, TServiceInterface
            where TGetResponse : IValidationErrors, new()
            where TPostResponse : IValidationErrors, new()
        {
            builder.Services.AddOpenApi();
            builder.Services.AddAuthorization();
            builder.Services.AddHttpContextAccessor();
            builder.Services.AddScoped<GaiaValues>(sp =>
                sp.GetRequiredService<IHttpContextAccessor>().HttpContext
                   .ThrowIfNull().GetRequestValues());
            builder.Services.AddJwtAuthentication(builder.Configuration);
            builder.Services.AddTransient<IStorageService, StorageService>();
            builder.Services.AddTransient<TServiceInterface, TService>();
            builder.Services.AddTransient<IDbMigrator, DbMigrator>(sp =>
                new(sp.GetRequiredService<IStorageService>().GetDbDirectory()
                   .Combine(name)));
            builder.Services.AddZeusDbContext(name);

            return builder;
        }
    }

}