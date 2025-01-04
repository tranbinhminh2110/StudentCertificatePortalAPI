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
        public readonly IEmailService _emailService;

        public PeerReviewService(IUnitOfWork uow, IMapper mapper, IEmailService emailService)
        {
            _uow = uow;
            _mapper = mapper;
            _emailService = emailService;
        }
        public async Task<PeerReviewDto> CreatePeerReviewAsync(CreatePeerReviewRequest request, CancellationToken cancellationToken)
        {

            var reviewedUser = await _uow.UserRepository.FirstOrDefaultAsync(x => x.UserId == request.ReviewedUserId);
            if (reviewedUser == null)
            {
                throw new KeyNotFoundException("User not found.");
            }

            var score = await _uow.ScoreRepository.FirstOrDefaultAsync(x => x.ScoreId == request.ScoreId);
            if (score == null)
            {
                throw new KeyNotFoundException("Score not found.");
            }

            var peerReview = await _uow.PeerReviewRepository.WhereAsync(x => x.ReviewedUserId == request.ReviewedUserId && x.ScoreId == request.ScoreId && x.ReviewerId == null);


            if (peerReview.Any())
            {
                throw new InvalidOperationException("You have already submitted a review for this user.");
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
            var result = await _uow.PeerReviewRepository.FirstOrDefaultAsync(x => x.PeerReviewId == peerReviewId, cancellationToken, include: i => i.Include(pr => pr.Score).Include(pr => pr.PeerReviewDetails));
            if (result == null) { throw new KeyNotFoundException("Peer Review not found."); }


            result.PeerReviewDetails.Clear();
            _uow.PeerReviewRepository.Delete(result);

            await _uow.Commit(cancellationToken);
            return _mapper.Map<PeerReviewDto>(result);
        }

        public async Task<List<PeerReviewDto>> GetAll(CancellationToken cancellationToken)
        {
            var result = await _uow.PeerReviewRepository.GetAll();
            return _mapper.Map<List<PeerReviewDto>>(result);
        }

        public async Task<List<PeerReviewForReviewedUserDto>> GetListPeerReviewAsyncForReviewedUser(int scoreId, CancellationToken cancellationToken)
        {
            var peerReviews = await _uow.PeerReviewRepository
                .WhereAsync(x => x.ScoreId == scoreId, cancellationToken,
                             include: query => query.Include(p => p.Reviewer)
                                                    .Include(p => p.Score)
                                                    .Include(p => p.Score.Exam));

            if (!peerReviews.Any())
            {
                throw new KeyNotFoundException("Your review has not been requested for peer review yet.");
            }

            var gradedPeerReviews = peerReviews.Where(x => x.ReviewerId != null).ToList();

            if (!gradedPeerReviews.Any())
            {
                throw new InvalidOperationException("None of your reviews have been graded yet.");
            }


            var peerReviewDtos = new List<PeerReviewForReviewedUserDto>();

            foreach (var peerReview in gradedPeerReviews)
            {
                var peerReviewDto = new PeerReviewForReviewedUserDto
                {
                    PeerReviewId = peerReview.PeerReviewId,
                    ReviewedUserId = peerReview.ReviewedUserId ?? 0,
                    ReviewerId = peerReview.ReviewerId ?? 0,
                    ReviewerName = peerReview.Reviewer?.Fullname ?? "Unknown",
                    ReviewDate = peerReview.ReviewDate,
                    ScoreId = peerReview.ScoreId,
                    MaxQuestionScore = await CheckPointEachQuestion(scoreId),
                    FeedbackPeerReviewer = peerReview.FeedbackPeerReviewer,
                    ScorePeerReviewer = peerReview.ScorePeerReviewer ?? 0,
                    ExamName = peerReview.Score?.Exam?.ExamName ?? "Unknown Exam",
                };

                peerReviewDtos.Add(peerReviewDto);
            }

            return peerReviewDtos;
        }





        public async Task<List<PeerReviewForReviewerDto>> GetListPeerReviewAsyncForReviewer(int examId, CancellationToken cancellationToken)
        {

            var listNeededPeerReview = await _uow.PeerReviewRepository
                .WhereAsync(
                    x => x.Score.ExamId == examId && x.ReviewerId == null,
                    cancellationToken,
                    include: query => query.Include(p => p.Score.User)
                                           .Include(p => p.Score.Exam)
                );

            if (!listNeededPeerReview.Any())
            {
                return new List<PeerReviewForReviewerDto>();
            }

            var listNeededPeerReviewDto = new List<PeerReviewForReviewerDto>();
            foreach (var peerReview in listNeededPeerReview)
            {
                var maxQuestionScore = peerReview.ScoreId != null
                    ? await CheckPointEachQuestion(peerReview.ScoreId)
                    : 0;

                listNeededPeerReviewDto.Add(new PeerReviewForReviewerDto
                {
                    PeerReviewId = peerReview.PeerReviewId,
                    ReviewedUserId = peerReview.ReviewedUserId ?? 0,
                    ReviewedUserName = peerReview.Score?.User?.Fullname ?? "Unknown",
                    ExamName = peerReview.Score?.Exam?.ExamName ?? "Unknown Exam",
                    ScoreId = peerReview.ScoreId,
                    MaxQuestionScore = maxQuestionScore
                });
            }

            return listNeededPeerReviewDto;
        }




        public async Task<PeerReviewDto> GetPeerReviewByIdAsync(int peerReviewId, Enums.EnumPeerReviewType peerType, CancellationToken cancellationToken)
        {
            var peerReview = await _uow.PeerReviewRepository.FirstOrDefaultAsync(
                x => x.PeerReviewId == peerReviewId,
                cancellationToken,
                include: x => x.Include(pr => pr.PeerReviewDetails));

            if (peerReview == null)
            {
                throw new KeyNotFoundException("Peer review not found.");
            }

            if (peerType == Enums.EnumPeerReviewType.View && (peerReview.ReviewerId == null || peerReview.ReviewerId <= 0))
            {
                throw new InvalidOperationException("This peer review has not been reviewed yet.");
            }


            var questionTypes = peerType == Enums.EnumPeerReviewType.Review
                ? new[] { Enums.EnumQuestionType.Essay.ToString() } 
                : new[] { Enums.EnumQuestionType.Essay.ToString(), Enums.EnumQuestionType.Choice.ToString() };


            var userAnswers = await _uow.UserAnswerRepository.WhereAsync(
                x => x.ScoreId == peerReview.ScoreId &&
                     questionTypes.Contains(x.QuestionType),
                cancellationToken,
                include: x => x.Include(q => q.Question)
                               .ThenInclude(q => q.Answers));
            if (!userAnswers.Any(x => x.QuestionType == Enums.EnumQuestionType.Essay.ToString()))
            {
                throw new InvalidOperationException("This peer review does not contain any essay questions.");
            }

            if (!userAnswers.Any())
            {
                throw new InvalidOperationException("The user has not submitted any answers.");
            }


            var pointsEachQuestion = await CheckPointEachQuestion(peerReview.ScoreId);

            var peerReviewDto = _mapper.Map<PeerReviewDto>(peerReview);

            if (peerType == Enums.EnumPeerReviewType.Review)
            {
                peerReviewDto.UserAnswers = userAnswers.Select(userAnswer =>
                {
                    var peerReviewDetail = peerReview.PeerReviewDetails
                        .FirstOrDefault(detail => detail.QuestionId == userAnswer.QuestionId);

                    return new UserAnswerForEssayDto
                    {
                        UserAnswerId = userAnswer.UserAnswerId,
                        QuestionId = userAnswer.QuestionId ?? 0,
                        QuestionName = userAnswer.Question?.QuestionText ?? "Unknown",
                        ScoreValue = peerReviewDetail?.ScoreEachQuestion ?? 0,
                        AnswerContent = userAnswer.AnswerContent,
                        FeedbackForEachQuestion = peerReviewDetail?.Feedback,
                    };
                }).ToList();
            }
            else if (peerType == Enums.EnumPeerReviewType.View)
            {

                peerReviewDto.QuestionReviews = userAnswers.Select(userAnswer =>
                {
                    var peerReviewDetail = peerReview.PeerReviewDetails
                        .FirstOrDefault(detail => detail.QuestionId == userAnswer.QuestionId);

                    return new QuestionReviewDto
                    {
                        QuestionId = userAnswer.QuestionId ?? 0,
                        QuestionName = userAnswer.Question?.QuestionText ?? "Unknown",
                        QuestionType = userAnswer.QuestionType,
                        UserAnswersForChoice = userAnswer.QuestionType == Enums.EnumQuestionType.Choice.ToString()
                                ? new List<int> { userAnswer.AnswerId ?? 0 } 
                                : null,
                        UserAnswerContentForEssay = userAnswer.QuestionType == Enums.EnumQuestionType.Essay.ToString()
                            ? userAnswer.AnswerContent
                            : null,
                        SystemAnswers = userAnswer.Question?.Answers.Select(a => new AnswerDto
                        {
                            AnswerId = a.AnswerId,
                            Text = a.Text, 
                            IsCorrect = a.IsCorrect
                        }).ToList() ?? new List<AnswerDto>(),
                        IsCorrectQuestion = peerReviewDetail?.ScoreEachQuestion > 0,
                        ScoreValue = userAnswer.QuestionType == Enums.EnumQuestionType.Essay.ToString() ? peerReviewDetail?.ScoreEachQuestion ?? 0 : userAnswer.ScoreValue ?? 0,
                        SubmittedAt = userAnswer.SubmittedAt
                    };
                }).ToList();
            }

            peerReviewDto.MaxQuestionScore = pointsEachQuestion;

            return peerReviewDto;
        }






        public async Task<PeerReviewDto> UpdatePeerReviewAsync(int peerReviewId, UpdatePeerReviewRequest request, CancellationToken cancellationToken)
        {

            var peerReview = await _uow.PeerReviewRepository.FirstOrDefaultAsync(
                x => x.PeerReviewId == peerReviewId,
                cancellationToken,
                include: x => x.Include(sc => sc.Score)
            );

            if (peerReview == null)
            {
                throw new KeyNotFoundException("PeerReview not found.");
            }

            if (peerReview.ReviewedUserId == request.ReviewerId)
            {
                throw new InvalidOperationException("You cannot review your own submission.");
            }

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


            peerReview.ReviewDate = DateTime.UtcNow;
            peerReview.FeedbackPeerReviewer = request.FeedbackPeerReviewer;
            peerReview.ReviewerId = request.ReviewerId;


            foreach (var questionScore in request.peerReviewQuestionScores)
            {

                var peerReviewDetail = await _uow.PeerReviewDetailRepository
                    .FirstOrDefaultAsync(x => x.PeerReviewId == peerReview.PeerReviewId && x.QuestionId == questionScore.QuestionId);

                if (peerReviewDetail != null)
                {

                    peerReviewDetail.ScoreEachQuestion = questionScore.ScoreForQuestion;
                    peerReview.ScorePeerReviewer += questionScore.ScoreForQuestion;
                    peerReviewDetail.Feedback = questionScore.FeedBackForQuestion;
                    _uow.PeerReviewDetailRepository.Update(peerReviewDetail);
                }
                else
                {
                    var newPeerReviewDetail = new PeerReviewDetail
                    {
                        PeerReviewId = peerReview.PeerReviewId,
                        QuestionId = questionScore.QuestionId,
                        Feedback = questionScore.FeedBackForQuestion,
                        ScoreEachQuestion = questionScore.ScoreForQuestion,
                        UserAnswerId = questionScore.UserAnswerId,
                    };
                    await _uow.PeerReviewDetailRepository.AddAsync(newPeerReviewDetail, cancellationToken);
                }
            }

            await _uow.Commit(cancellationToken);


            var reviewedUser = await _uow.UserRepository.FirstOrDefaultAsync(
                    x => x.UserId == peerReview.ReviewedUserId,
                    cancellationToken
                    );

            if (reviewedUser != null)
            {
                var emailSubject = "Peer Review Process Completed";
                var emailBody = $"Dear {reviewedUser.Fullname},\n\n" +
                                "We are pleased to inform you that your submission has been successfully reviewed as part of the peer review process.\n\n" +
                                "We sincerely appreciate your efforts and contributions to this initiative. Should you have any further inquiries or require additional information, please do not hesitate to reach out.\n\n" +
                                "Thank you for your participation and dedication.\n\n" +
                                "Best regards,\n" +
                                "Student Information Portal";

                await _emailService.SendEmailAsync(reviewedUser.Email, emailSubject, emailBody);
            }
            var createNewPeerReview = new CreatePeerReviewRequest
            {
                ReviewedUserId = peerReview.ReviewedUserId ?? 0,
                ScoreId = peerReview.ScoreId,
            };
            var newPeerReview = await CreatePeerReviewAsync(createNewPeerReview, cancellationToken);
            if (newPeerReview == null) throw new KeyNotFoundException("Creating new Peer Review not successful.");

            return _mapper.Map<PeerReviewDto>(peerReview);
        }

        private async Task<double> CheckPointEachQuestion(int scoreId)
        {
            var userAnswers = await _uow.UserAnswerRepository.WhereAsync(x => x.ScoreId == scoreId);
            if (!userAnswers.Any())
            {
                return 0; 
            }

            var scoreExam = await _uow.ScoreRepository.FirstOrDefaultAsync(
                x => x.ScoreId == scoreId,
                new CancellationToken(),
                include: x => x.Include(sc => sc.Exam)
            );

            if (scoreExam?.Exam == null || scoreExam.Exam.QuestionCount <= 0)
            {
                return 0; 
            }

            var questionCount = scoreExam.Exam.QuestionCount;

            var countToUse = userAnswers.Count() != questionCount ? questionCount : userAnswers.Count();

            return 100.0 / countToUse ?? 0;
        }
    }
}
