using StudentCertificatePortal_API.Contracts.Requests;
using StudentCertificatePortal_API.DTOs;

namespace StudentCertificatePortal_API.Services.Interface
{
    public interface IScoreService
    {
        Task<ScoreDto> Scoring(UserAnswerRequest request, CancellationToken cancellationToken);
    }
}
