using Gaia.Helpers;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Zeus.Helpers;

public static class ServiceProviderExtension
{
    public static T GetConfigurationSection<T>(this IServiceProvider serviceProvider, string path)
    {
        return serviceProvider.GetRequiredService<IConfiguration>().GetSection(path).Get<T>().ThrowIfNull(); 
    }
}