using System.Security.Claims;

namespace Zeus.Helpers;

public static class ClaimExtension
{
    extension(IEnumerable<Claim> claims)
    {
        public Claim GetClaim(string type)
        {
            var claimValues = claims.Where(x => x.Type == type).ToArray();

            if (claimValues.Length == 0)
            {
                throw new($"Not found claim {type}");
            }

            if (claimValues.Length > 1)
            {
                throw new($"Multi claims {type}");
            }

            return claimValues[0];
        }

        public Claim GetNameClaim()
        {
            return claims.GetClaim(ClaimTypes.Name);
        }

        public Claim GetNameIdentifierClaim()
        {
            return claims.GetClaim(ClaimTypes.NameIdentifier);
        }

        public Claim GetRoleClaim()
        {
            return claims.GetClaim(ClaimTypes.Role);
        }

        public string GetName()
        {
            return claims.GetNameClaim().Value;
        }
    }

}