using StudentCertificatePortal_API.Contracts.Requests;
using StudentCertificatePortal_API.DTOs;

namespace StudentCertificatePortal_API.Services.Interface
{
    public interface ICourseService
    {
        Task<CourseDto> CreateCourseAsync(CreateCourseRequest request, CancellationToken cancellationToken);
        Task<List<CourseDto>> GetAll();
        Task<CourseDto> GetCourseByIdAsync(int courseId, CancellationToken cancellationToken);
        Task<CourseDto> UpdateCourseAsync(int courseId, UpdateCourseRequest request, CancellationToken cancellationToken);
        Task<CourseDto> DeleteCourseAsync(int courseId, CancellationToken cancellationToken);
        Task<List<CourseDto>> GetCourseByNameAsync(string courseName, CancellationToken cancellationToken);
    }
}
