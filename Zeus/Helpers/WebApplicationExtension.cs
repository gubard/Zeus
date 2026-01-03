using Gaia.Helpers;
using Gaia.Services;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Zeus.Services;

namespace Zeus.Helpers;

public static class WebApplicationExtension
{
    public static async ValueTask RunZeusApp<
        TServiceInterface,
        TGetRequest,
        TPostRequest,
        TGetResponse,
        TPostResponse
    >(this WebApplication app)
        where TServiceInterface : class,
            IService<TGetRequest, TPostRequest, TGetResponse, TPostResponse>
        where TGetResponse : IValidationErrors, new()
        where TPostResponse : IValidationErrors, new()
    {
        if (app.Environment.IsDevelopment())
        {
            app.MapOpenApi();
        }

        app.UseHttpsRedirection();
        app.UseAuthentication();
        app.UseAuthorization();

        app.MapPost(
                RouteHelper.Get,
                (
                    TGetRequest request,
                    TServiceInterface authenticationService,
                    CancellationToken ct
                ) => authenticationService.GetAsync(request, ct)
            )
            .RequireAuthorization()
            .WithName(RouteHelper.GetName);

        app.MapPost(
                RouteHelper.Post,
                (
                    TPostRequest request,
                    TServiceInterface authenticationService,
                    CancellationToken ct
                ) => authenticationService.PostAsync(request, ct)
            )
            .RequireAuthorization()
            .WithName(RouteHelper.PostName);

        await app.Services.GetRequiredService<IZeusMigrator>().MigrateAsync(CancellationToken.None);
        await app.RunAsync();
    }
}
