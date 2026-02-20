using Microsoft.AspNetCore.Cors.Infrastructure;

namespace Zeus.Helpers;

public static class CorsOptionsExtension
{
    public static CorsOptions AddAllowAllPolicy(this CorsOptions options)
    {
        options.AddDefaultPolicy(policyBuilder =>
            policyBuilder.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader()
        );

        return options;
    }
}
