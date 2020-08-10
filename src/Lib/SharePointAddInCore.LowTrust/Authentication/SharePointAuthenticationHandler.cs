using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

using SharePointAddInCore.Core.SharePointContext;

using System.Text.Encodings.Web;
using System.Threading.Tasks;

namespace SharePointAddInCore.LowTrust.Authentication
{
    public class SharePointAuthenticationHandler : AuthenticationHandler<SharePointAuthenticationOptions>
    {
        private readonly ISharePointContext _sharePointContext;

        public SharePointAuthenticationHandler(
            IOptionsMonitor<SharePointAuthenticationOptions> options,
            ILoggerFactory logger,
            UrlEncoder encoder,
            ISystemClock clock,
            ISharePointContext sharePointContext) : base(options, logger, encoder, clock)
        {
            _sharePointContext = sharePointContext;
        }

        protected override async Task<AuthenticateResult> HandleAuthenticateAsync()
        {
            var userTokenResponse = await GetUserToken();
            if (userTokenResponse == null)
            {
                return AuthenticateResult.NoResult();
            }

            return AuthenticateResult.Success(
                new AuthenticationTicket(
                    userTokenResponse.ToClaimsPrincipal(SharePointAuthentication.SchemeName),
                    SharePointAuthentication.SchemeName));
        }

        private async Task<SharePointUserTokenResult> GetUserToken()
        {
            switch (Options.Target)
            {
                default:
                    return await _sharePointContext.GetUserAccessTokenForSPHost();
                case AuthenticationTarget.SPWebApp:
                    return await _sharePointContext.GetUserAccessTokenForSPAppWeb();
            }
        }
    }
}
