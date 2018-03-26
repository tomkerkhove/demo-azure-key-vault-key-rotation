using System;
using System.Threading.Tasks;
using Microsoft.Azure.KeyVault;
using Microsoft.Azure.Services.AppAuthentication;
using Microsoft.IdentityModel.Clients.ActiveDirectory;

namespace TomKerkhove.Demos.KeyVault.API.Builders
{
    public class KeyVaultAuthenticationBuilder
    {
        private static readonly string adApplicationId = "666ef5f5-017d-4f01-b105-54fea4d9618f";
        private static readonly string adApplicationSecret = "oKQTcEHlIZ7WKAiXqKt0DSC+i1HMOOueQnoHtXORpPs=";

        private readonly KeyVaultClient.AuthenticationCallback authenticationCallback;

        private KeyVaultAuthenticationBuilder(KeyVaultClient.AuthenticationCallback authenticationCallback)
        {
            this.authenticationCallback = authenticationCallback;
        }

        /// <summary>
        ///     Use basic authentication to authenticate with Azure AD
        /// </summary>
        public static KeyVaultAuthenticationBuilder UseBasicAuthentication()
        {
            return new KeyVaultAuthenticationBuilder(BasicAuthenticationCallback);
        }

        /// <summary>
        ///     Use Managed Service Identity to delegate authentication with Azure AD to Azure
        /// </summary>
        public static KeyVaultAuthenticationBuilder UseManagedServiceIdentity()
        {
            var azureServiceTokenProvider = new AzureServiceTokenProvider();
            var authenticationCallback = new KeyVaultClient.AuthenticationCallback(azureServiceTokenProvider.KeyVaultTokenCallback);
            return new KeyVaultAuthenticationBuilder(authenticationCallback);
        }

        /// <summary>
        ///     Build the Key Vault client
        /// </summary>
        public KeyVaultClient Build()
        {
            if (authenticationCallback == null)
            {
                throw new Exception("No authentication was configured to use");
            }

            return new KeyVaultClient(authenticationCallback);
        }

        private static async Task<string> BasicAuthenticationCallback(string authority, string resource, string scope)
        {
            var authContext = new AuthenticationContext(authority);
            var clientCredential = new ClientCredential(adApplicationId, adApplicationSecret);
            var token = await authContext.AcquireTokenAsync(resource, clientCredential).ConfigureAwait(continueOnCapturedContext: false);

            if (token == null)
            {
                throw new InvalidOperationException("Failed to obtain a token");
            }

            return token.AccessToken;
        }
    }
}