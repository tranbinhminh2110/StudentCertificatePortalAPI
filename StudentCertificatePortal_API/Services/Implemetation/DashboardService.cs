using StudentCertificatePortal_API.DTOs;
using StudentCertificatePortal_API.Enums;
using StudentCertificatePortal_API.Services.Interface;
using StudentCertificatePortal_Data.Models;
using StudentCertificatePortal_Repository.Interface;

namespace StudentCertificatePortal_API.Services.Implemetation
{
    public class DashboardService : IDashboardService
    {
        private readonly IUnitOfWork _uow;

        public DashboardService(IUnitOfWork uow)
        {
            _uow = uow;
        }
        public async Task<DashboardSummaryDto> GetDashboardSummaryAsync(CancellationToken cancellationToken)
        {
            var totalCertifications = await _uow.CertificationRepository.CountAsync(cancellationToken);
            var totalCourses = await _uow.CourseRepository.CountAsync(cancellationToken);
            var totalJobPositions = await _uow.JobPositionRepository.CountAsync(cancellationToken);
            var totalMajor = await _uow.MajorRepository.CountAsync(cancellationToken);
            var totalSimulationExam = await _uow.SimulationExamRepository.CountAsync(cancellationToken);
            var students = await _uow.UserRepository.WhereAsync(x => x.Role == "Student");
            var totalStudents = students.Count();

            return new DashboardSummaryDto()
            {
                TotalCertificates = totalCertifications,
                TotalCourses = totalCourses,
                TotalJobsPosition = totalJobPositions,
                TotalMajor = totalMajor,
                TotalSimulationExams = totalSimulationExam,
                TotalStudents = totalStudents,
            };

        }

        public async Task<decimal> GetTotalAmountAsync(TimePeriod period, DateTime? startDate = null, DateTime? endDate = null, CancellationToken cancellationToken = default)
        {
            IEnumerable<Payment> query = await _uow.PaymentRepository.GetAll();

            Func<DateTime?, DateTime?, bool> isValidDateRange = (start, end) =>
            {
                if (start.HasValue && end.HasValue)
                {
                    return start.Value.Year == end.Value.Year && start.Value.Month == end.Value.Month && start.Value.Day == end.Value.Day;
                }
                return true;
            };

            switch (period)
            {
                case TimePeriod.Week:
                    if (!startDate.HasValue || !endDate.HasValue || !isValidDateRange(startDate, endDate))
                    {
                        throw new ArgumentException("For Week period, startDate and endDate must be provided and within the same week.");
                    }
                    query = query.Where(p => p.PaymentDate >= startDate.Value && p.PaymentDate <= endDate.Value);
                    break;

                case TimePeriod.Month:
                    if (!startDate.HasValue || !endDate.HasValue)
                    {
                        var currentMonthStart = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);
                        var currentMonthEnd = currentMonthStart.AddMonths(1).AddDays(-1);
                        query = query.Where(p => p.PaymentDate >= currentMonthStart && p.PaymentDate <= currentMonthEnd);
                    }
                    else if (!isValidDateRange(startDate, endDate))
                    {
                        throw new ArgumentException("Start date and end date must be within the same month.");
                    }
                    else
                    {
                        query = query.Where(p => p.PaymentDate >= startDate.Value && p.PaymentDate <= endDate.Value);
                    }
                    break;

                case TimePeriod.Year:
                    if (!startDate.HasValue || !endDate.HasValue)
                    {
                        var currentYearStart = new DateTime(DateTime.Now.Year, 1, 1);
                        var currentYearEnd = new DateTime(DateTime.Now.Year, 12, 31);
                        query = query.Where(p => p.PaymentDate >= currentYearStart && p.PaymentDate <= currentYearEnd);
                    }
                    else if (startDate.Value.Year != endDate.Value.Year)
                    {
                        throw new ArgumentException("Start date and end date must be within the same year.");
                    }
                    else
                    {
                        query = query.Where(p => p.PaymentDate >= startDate.Value && p.PaymentDate <= endDate.Value);
                    }
                    break;

                case TimePeriod.Custom:
                    if (startDate.HasValue && endDate.HasValue)
                    {
                        query = query.Where(p => p.PaymentDate >= startDate.Value && p.PaymentDate <= endDate.Value);
                    }
                    else
                    {
                        throw new ArgumentException("For custom date range, both startDate and endDate must be provided.");
                    }
                    break;
            }
            return query.Sum(p => p.PaymentPoint ?? 0);
        }

    }
}
