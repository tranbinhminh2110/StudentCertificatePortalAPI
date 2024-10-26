using StudentCertificatePortal_API.Contracts.Requests;
using StudentCertificatePortal_API.DTOs;

namespace StudentCertificatePortal_API.Services.Interface
{
    public interface ICourseEnrollmentService
    {
        Task<CourseEnrollmentDto> CreateCourseEnrollmentAsync(CreateCourseEnrollmentRequest request, CancellationToken cancellationToken);
        Task<List<CourseEnrollmentDto>> GetAll();
        Task<CourseEnrollmentDto> GetCourseEnrollmentByIdAsync(int courseEnrollmentId, CancellationToken cancellationToken);
        Task<CourseEnrollmentDto> UpdateCourseEnrollmentAsync(int courseEnrollmentId, UpdateCourseEnrollmentRequest request, CancellationToken cancellationToken);
        Task<CourseEnrollmentDto> DeleteCourseEnrollmentAsync(int courseEnrollmentId, CancellationToken cancellationToken);
        Task<List<CourseEnrollmentDto>> GetCourseEnrollmentByUserIdAsync(int userId, CancellationToken cancellationToken);

    }
}
