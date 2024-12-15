namespace StudentCertificatePortal_API.Services.Interface
{
    public interface IMemoryCacheService
    {
        Task<T> GetAsync<T>(string key);
        Task SetAsync<T>(string key, T value, TimeSpan expiration);
    }
}
