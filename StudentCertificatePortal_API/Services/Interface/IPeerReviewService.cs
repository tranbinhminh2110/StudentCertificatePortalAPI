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

        Task<List<PeerReviewForReviewerDto>> GetListPeerReviewAsyncForReviewer(int examId, CancellationToken cancellationToken);
        Task<List<PeerReviewForReviewedUserDto>> GetListPeerReviewAsyncForReviewedUser(int scoreId, CancellationToken cancellationToken);
    }
}
