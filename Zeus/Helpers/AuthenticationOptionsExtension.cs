using System.Text;
using Gaia.Helpers;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using Zeus.Models;

namespace Zeus.Helpers;

public static class AuthenticationOptionsExtension
{
    public static AuthenticationOptions SetJwtBearerDefaults(this AuthenticationOptions options)
    {
        options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
        options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
        options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;

        return options;
    }
}

public static class JwtBearerOptionsExtension
{
    public static JwtBearerOptions SetJwtOptions(
        this JwtBearerOptions options,
        IConfiguration configuration
    )
    {
        var jwtOptions = configuration.GetConfigurationSection<JwtOptions>("Jwt");
        var key = Encoding.UTF8.GetBytes(jwtOptions.Key.ThrowIfNull());
        var symmetricSecurityKey = new SymmetricSecurityKey(key);

        options.TokenValidationParameters = new()
        {
            ValidIssuer = jwtOptions.Issuer.ThrowIfNull(),
            ValidAudience = jwtOptions.Audience.ThrowIfNull(),
            IssuerSigningKey = symmetricSecurityKey,
            ValidateIssuer = true,
            ValidateLifetime = true,
            ValidateAudience = true,
            ValidateIssuerSigningKey = true,
        };

        return options;
    }
}
