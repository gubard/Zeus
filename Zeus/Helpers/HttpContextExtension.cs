using System.Security.Claims;
using Gaia.Helpers;
using Gaia.Models;
using Gaia.Services;
using Microsoft.AspNetCore.Http;
using Zeus.Models;

namespace Zeus.Helpers;

public static class HttpContextExtension
{
    extension(HttpContext httpContext)
    {
        public GaiaValues GetRequestValues()
        {
            return new(httpContext.GetTimeZoneOffset(), Guid.Parse(httpContext.GetUserId()));
        }

        public Claim GetClaim(string type)
        {
            return httpContext.User.Claims.GetClaim(type);
        }

        public Claim GetNameIdentifierClaim()
        {
            return httpContext.GetClaim(ClaimTypes.NameIdentifier);
        }

        public Claim GetNameClaim()
        {
            return httpContext.GetClaim(ClaimTypes.Name);
        }

        public Claim GetRoleClaim()
        {
            return httpContext.GetClaim(ClaimTypes.Role);
        }

        public string GetTimeZoneOffsetRequestHeader()
        {
            return httpContext.GetRequestHeader(HttpHeader.TimeZoneOffset);
        }

        public string GetIdempotentIdHeader()
        {
            return httpContext.GetRequestHeader(HttpHeader.IdempotentId);
        }

        public string GetAuthorizationRequestHeader()
        {
            return httpContext.GetRequestHeader(HttpHeader.Authorization);
        }

        public TimeSpan GetTimeZoneOffset()
        {
            return TimeSpan.Parse(httpContext.GetTimeZoneOffsetRequestHeader());
        }

        public string GetRequestHeader(string name)
        {
            return httpContext.Request.Headers[name].Single().ThrowIfNull();
        }

        public Guid GetIdempotentId()
        {
            return Guid.Parse(httpContext.GetIdempotentIdHeader());
        }

        public string GetUserId()
        {
            var role = httpContext.GetRoleClaim().Value.ParseEnum<Role>();

            switch (role)
            {
                case Role.User:
                {
                    var nameIdentifier = httpContext.GetNameIdentifierClaim();

                    return nameIdentifier.Value;
                }
                case Role.Service:
                {
                    var nameIdentifier = httpContext
                        .Request.Headers[HttpHeader.UserId]
                        .Single()
                        .ThrowIfNull();

                    return nameIdentifier;
                }
                default:
                    throw new ArgumentOutOfRangeException(nameof(role), role, null);
            }
        }
    }
}
