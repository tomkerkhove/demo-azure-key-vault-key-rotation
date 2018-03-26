using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Azure.KeyVault;
using Microsoft.Extensions.Configuration;
using TomKerkhove.Demos.KeyVault.API.Builders;
using TomKerkhove.Demos.KeyVault.API.Providers.Interfaces;

namespace TomKerkhove.Demos.KeyVault.API.Providers
{
    public class SecretProvider : ISecretProvider
    {
        private readonly KeyVaultClient keyVaultClient;

        public SecretProvider()
        {
            var keyVaultAutenticationBuilder = Startup.IsDevelopment ? KeyVaultAuthenticationBuilder.UseBasicAuthentication() : KeyVaultAuthenticationBuilder.UseManagedServiceIdentity();
            keyVaultClient = keyVaultAutenticationBuilder.Build();
        }

        // You should never do this, but it's a demo so why bother!
        public string VaultUri { get; } = "https://secure-applications.vault.azure.net/";

        public async Task<string> GetSecretAsync(string secretName)
        {
            // Fetch latest secret from Key Vault
            var secret = await keyVaultClient.GetSecretAsync(VaultUri, secretName);

            return secret.Value;
        }
    }
}