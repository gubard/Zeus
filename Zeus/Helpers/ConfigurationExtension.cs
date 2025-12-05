using Gaia.Helpers;
using Microsoft.Extensions.Configuration;

namespace Zeus.Helpers;

public static class ConfigurationExtension
{
    public static T GetConfigurationSection<T>(this IConfiguration configuration, string path)
    {
        return configuration.GetSection(path).Get<T>().ThrowIfNull();
    }
}