using Gaia.Helpers;
using Gaia.Services;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Nestor.Db.Services;
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
        where TPostResponse : class, IValidationErrors, new()
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
                async (TGetRequest request, TServiceInterface service, CancellationToken ct) =>
                    await service.GetAsync(request, ct)
            )
            .RequireAuthorization()
            .WithName(RouteHelper.GetName);

        app.MapPost(
                RouteHelper.Post,
                async (
                    TPostRequest request,
                    TServiceInterface service,
                    IHttpContextAccessor accessor,
                    IIdempotenceService idempotenceService,
                    CancellationToken ct
                ) =>
                {
                    var idempotentId = accessor.HttpContext.ThrowIfNull().GetIdempotentId();
                    var value = await idempotenceService.GetAsync<TPostResponse>(idempotentId, ct);

                    if (value is not null)
                    {
                        return value;
                    }

                    value = await service.PostAsync(idempotentId, request, ct);
                    await idempotenceService.AddAsync(idempotentId, value, ct);

                    return value;
                }
            )
            .RequireAuthorization()
            .WithName(RouteHelper.PostName);

        await app.Services.GetRequiredService<IZeusMigrator>().MigrateAsync(CancellationToken.None);
        await app.RunAsync();
    }
}
