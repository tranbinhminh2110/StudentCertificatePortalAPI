using StudentCertificatePortal_API.Contracts.Requests;
using StudentCertificatePortal_API.DTOs;

namespace StudentCertificatePortal_API.Services.Interface
{
    public interface IPeerReviewService
    {
        Task<PeerReviewDto> CreatePeerReviewAsync(CreatePeerReviewRequest request, CancellationToken cancellationToken);
        Task<List<PeerReviewDto>> GetAll(CancellationToken cancellationToken);
        Task<PeerReviewDto> GetPeerReviewByIdAsync(int peerReviewId, CancellationToken cancellationToken);
        Task<PeerReviewDto> UpdatePeerReviewAsync(int peerReviewId, UpdatePeerReviewRequest request, CancellationToken cancellationToken);
        Task<PeerReviewDto> DeletePeerReviewAsync(int peerReviewId, CancellationToken cancellationToken);

        Task<List<PeerReviewDto>> GetListPeerReviewAsyncForExam(int examId, CancellationToken cancellationToken);
    }
}
