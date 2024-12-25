using StudentCertificatePortal_API.Contracts.Requests;
using StudentCertificatePortal_API.DTOs;
using StudentCertificatePortal_Data.Models;

namespace StudentCertificatePortal_API.Services.Interface
{
    public interface IExamSessionService
    {
        Task<ExamSessionDto> CreateExamSessionAsync(CreateExamSessionRequest request, CancellationToken cancellationToken);
        Task<List<ExamSessionDto>> GetAll();
        Task<ExamSessionDto> GetExamSessionByIdAsync(int sessionId, CancellationToken cancellationToken);
        Task<ExamSessionDto> UpdateExamSessionAsync(int sessionId, UpdateExamSessionRequest request, CancellationToken cancellationToken);
        Task<ExamSessionDto> DeleteExamSessionAsync(int sessionId, CancellationToken cancellationToken);
        Task<List<ExamSessionDto>> GetExamSessionByNameAsync(string sessionName, CancellationToken cancellationToken);
        Task<List<ExamSessionDto>> GetExamSessionBySessionDateAsync(DateTime sessionDate, CancellationToken cancellationToken);
        Task<List<ExamSessionDto>> GetExamSessionByCertIdAsync(int certId, CancellationToken cancellationToken);

    }
}
