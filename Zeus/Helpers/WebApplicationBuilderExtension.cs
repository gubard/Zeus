using Gaia.Helpers;
using Gaia.Services;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Zeus.Services;

namespace Zeus.Helpers;

public static class WebApplicationBuilderExtension
{
    public static async ValueTask
        RunZeusApp<TServiceInterface, TService, TGetRequest, TPostRequest,
            TGetResponse, TPostResponse>(
            this WebApplicationBuilder builder, string name)
        where TServiceInterface : class, IService<TGetRequest, TPostRequest,
            TGetResponse, TPostResponse>
        where TService : class, TServiceInterface
        where TGetResponse : IValidationErrors, new()
        where TPostResponse : IValidationErrors, new()
    {
        builder.Services.AddOpenApi();
        builder.Services.AddAuthorization();
        builder.Services.AddHttpContextAccessor();
        builder.Services.AddJwtAuthentication(builder.Configuration);
        builder.Services.AddTransient<IStorageService, StorageService>();
        builder.Services.AddTransient<TServiceInterface, TService>();
        builder.Services.AddTransient<IDbMigrator, DbMigrator>(sp =>
            new(sp.GetRequiredService<IStorageService>().GetDbDirectory()
               .Combine(name)));
        builder.Services.AddZeusDbContext(name);

        var app = builder.Build();

// Configure the HTTP request pipeline.
        if (app.Environment.IsDevelopment())
        {
            app.MapOpenApi();
        }

        app.UseHttpsRedirection();
        app.UseAuthentication();
        app.UseAuthorization();

        app.MapPost(RouteHelper.Get,
                (TGetRequest request,
                        TServiceInterface authenticationService,
                        CancellationToken ct) =>
                    authenticationService.GetAsync(request, ct))
           .RequireAuthorization()
           .WithName(RouteHelper.GetName);

        app.MapPost(RouteHelper.Post,
                (TPostRequest request,
                        TServiceInterface authenticationService,
                        CancellationToken ct) =>
                    authenticationService.PostAsync(request, ct))
           .RequireAuthorization()
           .WithName(RouteHelper.PostName);

        await app.Services.GetRequiredService<IDbMigrator>()
           .MigrateAsync(CancellationToken.None);
        await app.RunAsync();
    }
}