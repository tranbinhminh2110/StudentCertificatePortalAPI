using StudentCertificatePortal_API.DTOs;
using StudentCertificatePortal_API.Enums;

namespace StudentCertificatePortal_API.Services.Interface
{
    public interface IDashboardService
    {
        Task<DashboardSummaryDto> GetDashboardSummaryAsync(CancellationToken cancellationToken);
        Task<decimal> GetTotalAmountAsync(TimePeriod period, DateTime? startDate = null, DateTime? endDate = null, CancellationToken cancellationToken = default);
        Task<Dictionary<int, decimal>> GetMonthlyRevenueAsync(int year, CancellationToken cancellationToken = default);
        Task<Dictionary<int, decimal>> GetWeeklyRevenueAsync(int year, int month, CancellationToken cancellationToken = default);
        Task<StudentDataDto> GetPercentageDistribution();

        Task<Dictionary<int, decimal>> GetDailyRevenueAsync(int year, int month, CancellationToken cancellationToken = default);

    }
}
