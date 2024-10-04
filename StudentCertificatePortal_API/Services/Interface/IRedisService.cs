namespace StudentCertificatePortal_API.Services.Interface
{
    public interface IRedisService
    {
        Task SaveTokenAsync(int userId, string accessToken, string refreshToken);
        Task SaveResetTokenAsync(int userId, string token, TimeSpan expiration);
        Task<string> GetResetTokenAsync(int userId);
        Task DeleteResetTokenAsync(int userId);
    }
}
