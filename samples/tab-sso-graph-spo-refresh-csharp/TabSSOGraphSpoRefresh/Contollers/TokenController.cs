using Azure.Identity;
using Microsoft.SharePoint.Client;

namespace TabSSOGraphSpoRefresh.Contollers
{
    public class TokenController
    {
        public async Task<string> getSiteUser(ConfigOptions config, string userAssertion, string siteUrl, string userLogin)
        {
            var tenantId = config.TeamsFx.Authentication.OAuthAuthority.Remove(0, "https://login.microsoftonline.com/".Length);
            string token = await getSPOToken(config.TeamsFx.Authentication.ClientId, config.TeamsFx.Authentication.ClientSecret, tenantId, userAssertion);
            ClientContext context = new ClientContext(siteUrl);

            context.ExecutingWebRequest += (sender, args) =>
            {
                args.WebRequestExecutor.RequestHeaders["Authorization"] = "Bearer " + token;
            };

            var web = context.Web;            
            context.Load(web);
            var user = web.EnsureUser(userLogin);
            context.Load(user);
            context.ExecuteQuery();
            return user.Title;
        }
        private async Task<string> getOBOToken(string clientId, string clientSecret, string tenantId, string userAssertion)
        {
            var scopes = new[] { "https://graph.microsoft.com/.default", "offline_access" }; 

            // using Azure.Identity;
            var options = new OnBehalfOfCredentialOptions
            {
                AuthorityHost = AzureAuthorityHosts.AzurePublicCloud,
            };

            // This is the incoming token to exchange using on-behalf-of flow
            var oboToken = userAssertion;

            var onBehalfOfCredential = new OnBehalfOfCredential(
                tenantId, clientId, clientSecret, oboToken, options);

            var onBehalfOfGraph = await onBehalfOfCredential.GetTokenAsync(new Azure.Core.TokenRequestContext(scopes), new CancellationToken());
            return onBehalfOfGraph.Token;
        }

        private async Task<string> getSPOToken(string clientId, string clientSecret, string tenantId, string userAssertion)
        {
            var scopes = new[] { "https://mmoeller.sharepoint.com/AllSites.Write" };

            // using Azure.Identity;
            var options = new OnBehalfOfCredentialOptions
            {
                AuthorityHost = AzureAuthorityHosts.AzurePublicCloud,
            };

            // This is the incoming token to exchange using on-behalf-of flow
            var oboToken = userAssertion;

            var onBehalfOfCredential = new OnBehalfOfCredential(
                tenantId, clientId, clientSecret, oboToken, options);

            var onBehalfOfSPO = await onBehalfOfCredential.GetTokenAsync(new Azure.Core.TokenRequestContext(scopes), new CancellationToken());
            return onBehalfOfSPO.Token;
        }
    }
}
