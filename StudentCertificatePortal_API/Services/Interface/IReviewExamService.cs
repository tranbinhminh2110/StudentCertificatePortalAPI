using StudentCertificatePortal_API.DTOs;

namespace StudentCertificatePortal_API.Services.Interface
{
    public interface IReviewExamService
    {
        Task<ExamReviewDto> GetExamReviewAsync(int examId, int userId, int scoreId, CancellationToken cancellationToken);
    }
}
