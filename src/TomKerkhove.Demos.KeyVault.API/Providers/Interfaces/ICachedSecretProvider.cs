using System.Threading.Tasks;

namespace TomKerkhove.Demos.KeyVault.API.Providers.Interfaces
{
    public interface ICachedSecretProvider : ISecretProvider
    {
        /// <summary>
        ///     Gets the value for a specific secret that is cached
        /// </summary>
        /// <param name="secretName">Name of the secret to use</param>
        /// <param name="ignoreCache">Indication whether or not the cache should be ignored</param>
        Task<string> GetSecretAsync(string secretName, bool ignoreCache);
    }
}