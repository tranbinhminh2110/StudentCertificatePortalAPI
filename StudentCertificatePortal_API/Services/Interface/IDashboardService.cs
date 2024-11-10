﻿using StudentCertificatePortal_API.DTOs;
using StudentCertificatePortal_API.Enums;

namespace StudentCertificatePortal_API.Services.Interface
{
    public interface IDashboardService
    {
        Task<DashboardSummaryDto> GetDashboardSummaryAsync(CancellationToken cancellationToken);
        Task<decimal> GetTotalAmountAsync(TimePeriod period, DateTime? startDate = null, DateTime? endDate = null, CancellationToken cancellationToken = default);
    }
}
