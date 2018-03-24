using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Memory;
using TomKerkhove.Demos.KeyVault.API.Providers.Interfaces;

namespace TomKerkhove.Demos.KeyVault.API.Providers
{
    public class MemoryCachedSecretProvider : ICachedSecretProvider
    {
        // You should find a balance between caching it, but not too long to limit the risk of exposing it via memory dumps
        // We cache it very long for demo sake
        private readonly TimeSpan defaultCacheExpiryDuration = TimeSpan.FromDays(value: 4);
        private readonly MemoryCache memoryCache;
        private readonly ISecretProvider secretProvider;
        private readonly ITelemetryProvider telemetryProvider;

        public MemoryCachedSecretProvider(ISecretProvider secretProvider, ITelemetryProvider telemetryProvider)
        {
            this.secretProvider = secretProvider;
            this.telemetryProvider = telemetryProvider;

            memoryCache = CreateMemoryCache();
        }

        public async Task<string> GetSecretAsync(string secretName)
        {
            return await GetSecretAsync(secretName, ignoreCache: false);
        }

        public async Task<string> GetSecretAsync(string secretName, bool ignoreCache)
        {
            // Check if the secret is already cached, return it if it is
            if (ignoreCache == false && memoryCache.TryGetValue(secretName, out string secretValue))
            {
                telemetryProvider.IncreaseGauge("Cache Hit");
                telemetryProvider.LogTrace($"Secret '{secretName}' cached");
                return secretValue;
            }

            // Fetch latest secret from Key Vault
            var secret = await secretProvider.GetSecretAsync(secretName);

            // Store found secret in memory cache
            telemetryProvider.IncreaseGauge("Cache Miss");
            telemetryProvider.LogTrace($"Secret '{secretName}' was not cached");
            memoryCache.Set(secretName, secret, defaultCacheExpiryDuration);

            return secret;
        }

        private static MemoryCache CreateMemoryCache()
        {
            var memoryCacheOptions = new MemoryCacheOptions();
            return new MemoryCache(memoryCacheOptions);
        }
    }
}