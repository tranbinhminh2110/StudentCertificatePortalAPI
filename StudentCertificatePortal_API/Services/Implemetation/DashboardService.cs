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
            var totalPoint = (await _uow.PaymentRepository.GetAll()).Select(p => p.PaymentPoint).Sum();
            var totalAmountOfTopUp = (await _uow.TransactionRepository.WhereAsync(x => x.TransStatus == Enums.EnumTransaction.Success.ToString())).Select(t => t.Amount).Sum();
            var totalAmoutOfRefund = (await _uow.TransactionRepository.WhereAsync(x => x.TransStatus == Enums.EnumTransaction.Refunded.ToString())).Select(t => t.Amount).Sum();

            return new DashboardSummaryDto()
            {
                TotalCertificates = totalCertifications,
                TotalCourses = totalCourses,
                TotalJobsPosition = totalJobPositions,
                TotalMajor = totalMajor,
                TotalSimulationExams = totalSimulationExam,
                TotalStudents = totalStudents,
                TotalPoint = totalPoint ?? 0,
                TotalAmountOfTopUp = totalAmountOfTopUp,
                TotalAmoutOfRefund = totalAmoutOfRefund,
            };

        }

        public async Task<StudentDataDto> GetPercentageDistribution()
        {
            var totalStudents = await _uow.UserRepository.WhereAsync(x => x.Role == "Student");
            int totalStudentCount = totalStudents.Count();

            var enrolledExamStudents = await _uow.ExamEnrollmentRepository
                                      .WhereAsync(x => x.ExamEnrollmentStatus == Enums.EnumExamEnrollment.Completed.ToString());

            var listExamEnrolled = enrolledExamStudents
                           .Select(x => x.UserId)
                           .Distinct()
                           .ToList();

            var enrolledCourseStudents = await _uow.CourseEnrollmentRepository
                                      .WhereAsync(x => x.CourseEnrollmentStatus == Enums.EnumCourseEnrollment.Completed.ToString());

            var listCourseEnrolled = enrolledCourseStudents
                           .Select(x => x.UserId)
                           .Distinct()
                           .ToList();

            var studentsInBoth = listExamEnrolled.Intersect(listCourseEnrolled).ToList();
            int studentsInBothCount = studentsInBoth.Count();


            var onlyCourseEnrolled = listCourseEnrolled.Except(studentsInBoth).Count();

            var onlyExamEnrolled = listExamEnrolled.Except(studentsInBoth).Count();
            var noEnrollment = totalStudentCount - listExamEnrolled.Count() - listCourseEnrolled.Count() + studentsInBothCount;


            var studentDataDto = new StudentDataDto
            {
                TotalStudents = totalStudentCount,
                PercentageTotalStudents = 100,
                OnlyEnrolledInCourse = onlyCourseEnrolled,
                PercentageOnlyEnrolledInCourse = (onlyCourseEnrolled / (double)totalStudentCount) * 100,
                OnlyPurchaseSimulationExams = onlyExamEnrolled,
                PercentageOnlyPurchaseSimulationExams = (onlyExamEnrolled / (double)totalStudentCount) * 100,
                PurchaseBoth = studentsInBothCount,
                PercentagePurchaseBoth = (studentsInBothCount / (double)totalStudentCount) * 100,
                PurchaseNothing = noEnrollment,
                PercentagePurchaseNothing = (noEnrollment / (double)totalStudentCount) * 100
            };

            return studentDataDto;
        }



        public async Task<Dictionary<int, decimal>> GetMonthlyRevenueAsync(int year, CancellationToken cancellationToken = default)
        {
            IEnumerable<Payment> query = await _uow.PaymentRepository.GetAll();

            query = query.Where(p => p.PaymentDate.HasValue && p.PaymentDate.Value.Year == year);

            var monthlyRevenue = new Dictionary<int, decimal>();

            for (int month = 1; month <= 12; month++)
            {
                var monthStart = new DateTime(year, month, 1);
                var monthEnd = monthStart.AddMonths(1).AddDays(-1);

                decimal totalForMonth = query
                    .Where(p => p.PaymentDate >= monthStart && p.PaymentDate <= monthEnd)
                    .Sum(p => p.PaymentPoint ?? 0);
                monthlyRevenue[month] = totalForMonth;
            }

            return monthlyRevenue;
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
                        var monthStartDate = startDate.Value;
                        var monthEndDate = monthStartDate.AddMonths(1).AddDays(-1);
                        query = query.Where(p => p.PaymentDate >= monthStartDate && p.PaymentDate <= monthEndDate);
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
                        var currentYearStart = startDate.Value;
                        var currentYearEnd = currentYearStart.AddYears(1).AddDays(-1);
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

        public async Task<Dictionary<int, decimal>> GetWeeklyRevenueAsync(int year, int month, CancellationToken cancellationToken = default)
        {
            IEnumerable<Payment> query = await _uow.PaymentRepository.GetAll();

            query = query.Where(p => p.PaymentDate.HasValue &&
                                     p.PaymentDate.Value.Year == year &&
                                     p.PaymentDate.Value.Month == month);

            var weeklyRevenue = new Dictionary<int, decimal>();

            int currentWeek = 1;
            DateTime weekStart = new DateTime(year, month, 1);

            while (weekStart.Month == month)
            {
                DateTime weekEnd = weekStart.AddDays(6);
                if (weekEnd.Month != month)
                {
                    weekEnd = new DateTime(year, month, DateTime.DaysInMonth(year, month));
                }

                decimal totalForWeek = query
                    .Where(p => p.PaymentDate >= weekStart && p.PaymentDate <= weekEnd)
                    .Sum(p => p.PaymentPoint ?? 0);

                weeklyRevenue[currentWeek] = totalForWeek;

                weekStart = weekStart.AddDays(7);
                currentWeek++;
            }

            return weeklyRevenue;
        }
    }
}
