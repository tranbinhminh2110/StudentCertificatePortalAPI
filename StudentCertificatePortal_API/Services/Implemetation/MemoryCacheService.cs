using Microsoft.Extensions.Caching.Memory;
using StudentCertificatePortal_API.Services.Interface;

namespace StudentCertificatePortal_API.Services.Implemetation
{
    public class MemoryCacheService: IMemoryCacheService
    {
        private readonly IMemoryCache _memoryCache;

        public MemoryCacheService(IMemoryCache memoryCache)
        {
            _memoryCache = memoryCache;
        }

        public async Task<T> GetAsync<T>(string key)
        {
            return _memoryCache.TryGetValue(key, out T value) ? value : default(T);
        }

        public async Task SetAsync<T>(string key, T value, TimeSpan expiration)
        {
            _memoryCache.Set(key, value, expiration);
        }
    }
}
