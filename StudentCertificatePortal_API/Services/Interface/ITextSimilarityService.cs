using StudentCertificatePortal_API.Contracts.Requests;

namespace StudentCertificatePortal_API.Services.Interface
{
    public interface ITextSimilarityService
    {
        Task<double> GetSimilarityScoreAsync(CompareAnswersRequest request);
    }
}
