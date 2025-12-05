using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Zeus.Helpers;

public static class ServiceCollectionExtension
{
    public static IServiceCollection AddJwtAuthentication(
        this IServiceCollection serviceCollection,
        IConfiguration configuration
    )
    {
        serviceCollection.AddAuthentication(x => x.SetJwtBearerDefaults())
           .AddJwtBearer(x => x.SetJwtOptions(configuration));

        return serviceCollection;
    }
}