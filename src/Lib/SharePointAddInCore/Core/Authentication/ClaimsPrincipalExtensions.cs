using System.Security.Claims;

namespace SharePointAddInCore.Core.Authentication
{
    public static class ClaimsPrincipalExtensions
    {
        public static string GetUserAccessToken(this ClaimsPrincipal user)
            => user.FindFirst(SharePointAuthentication.AccessTokenClaim)?.Value;

        public static string GetUserId(this ClaimsPrincipal user)
            => user.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        public static string GetUserDisplayName(this ClaimsPrincipal user)
            => user.FindFirst(ClaimTypes.Name)?.Value;

        public static string GetUserEmail(this ClaimsPrincipal user)
            => user.FindFirst(ClaimTypes.Email)?.Value;

        public static string GetUpn(this ClaimsPrincipal user)
            => user.FindFirst(ClaimTypes.Upn)?.Value;
    }
}
