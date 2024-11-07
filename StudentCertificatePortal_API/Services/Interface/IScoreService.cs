using StudentCertificatePortal_API.Contracts.Requests;
using StudentCertificatePortal_API.DTOs;

namespace StudentCertificatePortal_API.Services.Interface
{
    public interface IScoreService
    {
        Task<ScoreDto> Scoring(UserAnswerRequest request, CancellationToken cancellationToken);
        Task<List<ScoreDto>> GetScoreByUserId(int userId, int? examId, CancellationToken cancellationToken);
    }
}
