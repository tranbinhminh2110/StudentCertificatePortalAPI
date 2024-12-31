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
            var reviewer = await _uow.UserRepository.FirstOrDefaultAsync(x => x.UserId == request.ReviewedUserId);
            if (reviewer == null)
            {
                throw new KeyNotFoundException("Reviewer not found.");
            }

            var score = await _uow.ScoreRepository.FirstOrDefaultAsync(x => x.ScoreId == request.ScoreId);
            if (score == null)
            {
                throw new KeyNotFoundException("Score not found.");
            }

            var peerReviewDto = new PeerReview()
            {
                ReviewedUserId = request.ReviewedUserId,
                ScoreId = request.ScoreId,
                ReviewDate = DateTime.UtcNow,
                ScorePeerReviewer = 0,
                FeedbackPeerReviewer = string.Empty
            };

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

            var peerReview = await _uow.PeerReviewRepository.FirstOrDefaultAsync(x => x.PeerReviewId == peerReviewId);
            if (peerReview == null)
            {
                throw new KeyNotFoundException("Peer review not found.");
            }


            var userAnswers = await _uow.UserAnswerRepository.WhereAsync(x => x.ScoreId == peerReview.ScoreId && x.QuestionType == Enums.EnumQuestionType.Essay.ToString(), cancellationToken,
                include: x => x.Include(q => q.Question));

            var peerReviewDto = _mapper.Map<PeerReviewDto>(peerReview);

            peerReviewDto.UserAnswers = userAnswers.Select(userAnswer => new UserAnswerForEssayDto
            {
                UserAnswerId = userAnswer.UserAnswerId,
                QuestionId = userAnswer.QuestionId ?? 0,
                QuestionName = userAnswer.Question?.QuestionText ?? "Unknown",
                ScoreValue = userAnswer.ScoreValue ?? 0,
                AnswerContent = userAnswer.AnswerContent
            }).ToList();

            return peerReviewDto;
        }


        public async Task<PeerReviewDto> UpdatePeerReviewAsync(int peerReviewId, UpdatePeerReviewRequest request, CancellationToken cancellationToken)
        {
            // Retrieve the peer review with the associated score
            var peerReview = await _uow.PeerReviewRepository.FirstOrDefaultAsync(
                x => x.PeerReviewId == peerReviewId,
                cancellationToken,
                include: x => x.Include(sc => sc.Score)
            );

            if (peerReview == null)
            {
                throw new KeyNotFoundException("PeerReview not found.");
            }

            // Check if the reviewer is reviewing their own submission
            if (peerReview.ReviewedUserId == request.ReviewerId)
            {
                throw new InvalidOperationException("You cannot review your own submission.");
            }


            // Check if the user has passed the exam
            var examId = peerReview.Score.ExamId;
            var userScores = await _uow.ScoreRepository
    .WhereAsync(x => x.UserId == request.ReviewerId && x.ExamId == examId);

            var exam = await _uow.SimulationExamRepository.FirstOrDefaultAsync(
                x => x.ExamId == examId,
                cancellationToken
            );

            if (exam == null)
            {
                throw new Exception("Exam not found.");
            }



            if (!userScores.Any(x => x.ScoreValue >= exam.PassingScore))
            {
                throw new InvalidOperationException("The user has not passed the exam and cannot be reviewed.");
            }

            var peerReviewOfReviewer = await _uow.PeerReviewRepository.FirstOrDefaultAsync(
                x => x.ReviewerId == request.ReviewerId && x.ReviewerId == peerReview.ReviewerId,
                cancellationToken
            );
            if (peerReviewOfReviewer != null)
            {
                // Update the main peer review properties
                peerReviewOfReviewer.ReviewDate = DateTime.UtcNow;
                peerReviewOfReviewer.ScorePeerReviewer = request.ScorePeerReviewer;
                peerReviewOfReviewer.FeedbackPeerReviewer = request.FeedbackPeerReviewer;
                peerReviewOfReviewer.ReviewerId = request.ReviewerId;

                // Iterate over each question score in the request
                foreach (var questionScore in request.peerReviewQuestionScores)
                {
                    // Check if the PeerReviewDetail already exists for the specific PeerReviewId and QuestionId
                    var existingPeerReviewDetail = await _uow.PeerReviewDetailRepository
                        .FirstOrDefaultAsync(x => x.PeerReviewId == peerReviewOfReviewer.PeerReviewId && x.QuestionId == questionScore.QuestionId, cancellationToken);

                    if (existingPeerReviewDetail != null)
                    {
                        // If a PeerReviewDetail exists for this question, update it
                        existingPeerReviewDetail.ScoreEachQuestion = questionScore.ScoreForQuestion;
                        existingPeerReviewDetail.Feedback = questionScore.FeedBackForQuestion;
                        _uow.PeerReviewDetailRepository.Update(existingPeerReviewDetail);
                    }
                    else
                    {
                        // If no PeerReviewDetail exists for this question, create a new one
                        var newPeerReviewDetail = new PeerReviewDetail
                        {
                            PeerReviewId = peerReviewOfReviewer.PeerReviewId,
                            QuestionId = questionScore.QuestionId,
                            Feedback = questionScore.FeedBackForQuestion,
                            ScoreEachQuestion = questionScore.ScoreForQuestion
                        };

                        await _uow.PeerReviewDetailRepository.AddAsync(newPeerReviewDetail);
                    }
                }
            }


            // Proceed with the update
            peerReview.ReviewDate = DateTime.UtcNow;
            peerReview.ScorePeerReviewer = request.ScorePeerReviewer;
            peerReview.FeedbackPeerReviewer = request.FeedbackPeerReviewer;
            peerReview.ReviewerId = request.ReviewerId;

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

            // Map to DTO and return
            return _mapper.Map<PeerReviewDto>(peerReview);
        }





    }
}
