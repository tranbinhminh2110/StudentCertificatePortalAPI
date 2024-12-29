using AutoMapper;
using Microsoft.EntityFrameworkCore;
using StudentCertificatePortal_API.Contracts.Requests;
using StudentCertificatePortal_API.DTOs;
using StudentCertificatePortal_API.Services.Interface;
using StudentCertificatePortal_Data.Models;
using StudentCertificatePortal_Repository.Interface;

namespace StudentCertificatePortal_API.Services.Implemetation
{
    public class PeerReviewService : IPeerReviewService
    {
        private readonly IUnitOfWork _uow;
        private readonly IMapper _mapper;

        public PeerReviewService(IUnitOfWork uow, IMapper mapper)
        {
            _uow = uow;
            _mapper = mapper;
        }
        public async Task<PeerReviewDto> CreatePeerReviewAsync(CreatePeerReviewRequest request, CancellationToken cancellationToken)
        {
            // Kiểm tra xem ScoreId và ReviewerId có hợp lệ không
            var reviewer = await _uow.UserRepository.FirstOrDefaultAsync(x => x.UserId == request.ReviewerId);
            if (reviewer == null)
            {
                throw new KeyNotFoundException("Reviewer not found.");
            }

            var score = await _uow.ScoreRepository.FirstOrDefaultAsync(x => x.ScoreId == request.ScoreId);
            if (score == null)
            {
                throw new KeyNotFoundException("Score not found.");
            }

            // Tạo PeerReview mới
            var peerReviewDto = new PeerReview()
            {
                ReviewerId = request.ReviewerId,
                ScoreId = request.ScoreId,
                ReviewDate = DateTime.UtcNow,  
                ScorePeerReviewer = 0,       
                FeedbackPeerReviewer = string.Empty 
            };

            // Thêm vào cơ sở dữ liệu
            var result = await _uow.PeerReviewRepository.AddAsync(peerReviewDto);
            await _uow.Commit(cancellationToken);

            return _mapper.Map<PeerReviewDto>(result);
        }


        public async Task<PeerReviewDto> DeletePeerReviewAsync(int peerReviewId, CancellationToken cancellationToken)
        {
            var result = await _uow.PeerReviewRepository.FirstOrDefaultAsync(x => x.PeerReviewId == peerReviewId, cancellationToken, include: i => i.Include(pr => pr.Score));
            if (result == null) { throw new KeyNotFoundException("Peer Review not found."); }

             _uow.PeerReviewRepository.Delete(result);

            await _uow.Commit(cancellationToken);
            return _mapper.Map<PeerReviewDto>(result);
        }

        public async Task<List<PeerReviewDto>> GetAll(CancellationToken cancellationToken)
        {
            var result = await _uow.PeerReviewRepository.GetAll();
            return _mapper.Map<List<PeerReviewDto>>(result);
        }

        public async Task<PeerReviewDto> GetPeerReviewByIdAsync(int peerReviewId, CancellationToken cancellationToken)
{
    // Lấy PeerReview theo ID
    var peerReview = await _uow.PeerReviewRepository.FirstOrDefaultAsync(x => x.PeerReviewId == peerReviewId);
    if (peerReview == null)
    {
        throw new KeyNotFoundException("Peer review not found.");
    }

    // Lấy tất cả UserAnswers của người dùng dựa trên ScoreId
    var userAnswers = await _uow.UserAnswerRepository.WhereAsync(x => x.ScoreId == peerReview.ScoreId && x.QuestionType == Enums.EnumQuestionType.Essay.ToString(), cancellationToken, 
        include: x => x.Include(q => q.Question));


    // Chuyển đổi PeerReview thành DTO
    var peerReviewDto = _mapper.Map<PeerReviewDto>(peerReview);

    peerReviewDto.UserAnswers = userAnswers.Select(userAnswer => new UserAnswerForEssayDto
    {
        UserAnswerId = userAnswer.UserAnswerId,
        QuestionId = userAnswer.QuestionId?? 0,
        QuestionName = userAnswer.Question?.QuestionText ?? "Unknown",
        ScoreValue = userAnswer.ScoreValue ?? 0,
        AnswerContent = userAnswer.AnswerContent
    }).ToList();

    return peerReviewDto;
}


        public async Task<PeerReviewDto> UpdatePeerReviewAsync(int peerReviewId, UpdatePeerReviewRequest request, CancellationToken cancellationToken)
        {
            var peerReview = await _uow.PeerReviewRepository.FirstOrDefaultAsync(x => x.PeerReviewId == peerReviewId);
            if (peerReview == null)
            {
                throw new KeyNotFoundException("PeerReview not found.");
            }

            peerReview.ReviewDate = DateTime.UtcNow;
            peerReview.ScorePeerReviewer = request.ScorePeerReviewer;
            peerReview.FeedbackPeerReviewer = request.FeedbackPeerReviewer;

            foreach (var questionScore in request.peerReviewQuestionScores)
            {
                var userAnswer = await _uow.UserAnswerRepository
                    .FirstOrDefaultAsync(x => x.QuestionId == questionScore.QuestionId && x.ScoreId == peerReview.ScoreId);

                if (userAnswer != null)
                {
                    userAnswer.ScoreValue = questionScore.ScoreForQuestion;   
                    _uow.UserAnswerRepository.Update(userAnswer);
                }
                else
                {
                    throw new Exception("The user's answer has been deleted.");
                }
            }
            await _uow.Commit(cancellationToken);

            return _mapper.Map<PeerReviewDto>(peerReview);
        }

    }
}
