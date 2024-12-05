using Microsoft.AspNetCore.Mvc;
using StudentCertificatePortal_API.Commons;
using StudentCertificatePortal_API.Contracts.Requests;
using StudentCertificatePortal_API.DTOs;

namespace StudentCertificatePortal_API.Services.Interface
{
    public interface IFeedbackService
    {
        Task<FeedbackDto> CreateFeedbackAsync(CreateFeedbackRequest request, CancellationToken cancellationToken);
        Task<List<FeedbackDto>> GetAll();
        Task<FeedbackDto> GetFeedbackByIdAsync(int feedbackId, CancellationToken cancellationToken);
        Task<FeedbackDto> UpdateFeedbackAsync(int feedbackId, UpdateFeedbackRequest request, CancellationToken cancellationToken);
        Task<FeedbackDto> DeleteFeedbackAsync(int feedbackId, CancellationToken cancellationToken);
        Task<List<FeedbackDto>> GetFeedbackByUserIdAsync(int userId, CancellationToken cancellationToken);
        Task<List<FeedbackDto>> GetFeedbackByExamIdAsync(int examId, CancellationToken cancellationToken);
        Task<List<FeedbackDto>> GetFeedbackByCertIdAsync(int certId, CancellationToken cancellationToken);
        Task<AverageRatingDto> CalculateAverageFeedbackRatingAsync(int simulationExamId, CancellationToken cancellationToken);

    }
}
