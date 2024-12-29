using StudentCertificatePortal_API.Contracts.Requests;
using StudentCertificatePortal_API.Contracts.Responses;
using StudentCertificatePortal_API.DTOs;

namespace StudentCertificatePortal_API.Services.Interface
{
    public interface IExamEnrollmentService
    {
        Task<ExamEnrollmentResponse> CreateExamEnrollmentAsync(CreateExamEnrollmentRequest request, CancellationToken cancellationToken);
        Task<List<ExamEnrollmentDto>> GetAll();
        Task<ExamEnrollmentDto> GetExamEnrollmentById(int examEnrollmentId, CancellationToken cancellationToken);
        Task<List<ExamEnrollmentDto>> GetExamEnrollmentByUserId(int userId, CancellationToken cancellationToken);
        Task<ExamEnrollmentDto> UpdateExamEnrollmentAsync(int examEnrollmentId, UpdateExamEnrollmentRequest request, CancellationToken cancellationToken);
        Task<ExamEnrollmentDto> DeleteExamEnrollmentAsync(int examEnrollmentId, CancellationToken cancellationToken);

        /*Task<List<ExamEnrollmentDto>> GetExamEnrollmentByNameAsync(string examEnrollmentName, CancellationToken cancellationToken);*/
        Task<ExamEnrollmentResponse> CreateExamEnrollmentVoucherAsync(CreateExamEnrollmentVoucherRequest request, CancellationToken cancellationToken);
    }
}
