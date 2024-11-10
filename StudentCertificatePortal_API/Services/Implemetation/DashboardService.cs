using StudentCertificatePortal_API.DTOs;
using StudentCertificatePortal_API.Services.Interface;
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
    }
}
