using AutoMapper;
using Microsoft.EntityFrameworkCore;
using MimeKit.Cryptography;
using StudentCertificatePortal_API.Contracts.Requests;
using StudentCertificatePortal_API.DTOs;
using StudentCertificatePortal_API.Services.Interface;
using StudentCertificatePortal_Data.Models;
using StudentCertificatePortal_Repository.Interface;

namespace StudentCertificatePortal_API.Services.Implemetation
{
    public class ScoreService : IScoreService
    {
        private readonly IUnitOfWork _uow;
        private readonly IMapper _mapper;
        private readonly ITextSimilarityService _similarityService;

        public ScoreService(IUnitOfWork uow, IMapper mapper, ITextSimilarityService similarityService)
        {
            _uow = uow;
            _mapper = mapper;
            _similarityService = similarityService;
        }
        public async Task<ScoreDto> Scoring(UserAnswerRequest request, CancellationToken cancellationToken)
        {
            double totalScore = 0;
            var exam = await _uow.SimulationExamRepository.FirstOrDefaultAsync(x => x.ExamId == request.ExamId);
            var user = await _uow.UserRepository.FirstOrDefaultAsync(x => x.UserId == request.UserId);
            if (user is null) throw new KeyNotFoundException("User not found!");
            if (exam == null) throw new KeyNotFoundException("Simulation not found!");

            var enroll = await _uow.ExamEnrollmentRepository.WhereAsync(x => x.UserId == user.UserId && x.StudentOfExams.Any(x => x.ExamId == exam.ExamId));
            if (!enroll.Any())
            {
                throw new InvalidOperationException("User is not enrolled in this exam.");
            }

            double numberQuestion = exam.QuestionCount ?? 0;
            double pointsPerQuestion = 100 / numberQuestion;
            var scoreEntity = new Score()
            {
                UserId = request.UserId,
                ExamId = request.ExamId,
            };
            var result = await _uow.ScoreRepository.AddAsync(scoreEntity);
            await _uow.Commit(cancellationToken);
            foreach (var model in request.QuestionRequests)
            {
                if (model.QuestionType == Enums.EnumQuestionType.Essay)
                {
                    var essayScore = await CheckAnswerEssay(model.QuestionId, model.UserAnswerText, pointsPerQuestion, cancellationToken);
                    totalScore += essayScore;
                    var userAnswer = new UserAnswer()
                    {
                        UserId = user.UserId,
                        ExamId = exam.ExamId,
                        QuestionId = model.QuestionId,
                        AnswerContent = model.UserAnswerText,
                        ScoreValue = (decimal)essayScore,
                        SubmittedAt = DateTime.UtcNow,
                        QuestionType = "Essay",
                        IsCorrect = essayScore >= (pointsPerQuestion / 2) ? true : false,
                        ScoreId = scoreEntity.ScoreId,
                        
                    };

                    var userAnswerResult = await _uow.UserAnswerRepository.AddAsync(userAnswer);
                    await _uow.Commit(cancellationToken);
                }
                else
                {
                    bool checkQuestion = await CheckAnswerCorrect(model.QuestionId, model.UserAnswerId, cancellationToken);

                    if (checkQuestion)
                    {
                        totalScore += pointsPerQuestion;
                        foreach(var index in model.UserAnswerId)
                        {
                            var userAnswer = new UserAnswer()
                            {
                                UserId = user.UserId,
                                ExamId = exam.ExamId,
                                QuestionId = model.QuestionId,
                                AnswerId = index,
                                ScoreValue = (decimal)pointsPerQuestion,
                                SubmittedAt = DateTime.UtcNow,
                                IsCorrect = true,
                                QuestionType = "Choice",
                                ScoreId= scoreEntity.ScoreId,
                            };
                            var userAnswerResult = await _uow.UserAnswerRepository.AddAsync(userAnswer);
                            
                        }
                        await _uow.Commit(cancellationToken);

                    }
                    else
                    {
                        foreach (var index in model.UserAnswerId)
                        {
                            var userAnswer = new UserAnswer()
                            {
                                UserId = user.UserId,
                                ExamId = exam.ExamId,
                                QuestionId = model.QuestionId,
                                AnswerId = index,
                                ScoreValue = 0,
                                SubmittedAt = DateTime.UtcNow,
                                IsCorrect = false,
                                QuestionType = "Choice",
                                ScoreId= scoreEntity.ScoreId,
                            };
                            var userAnswerResult = await _uow.UserAnswerRepository.AddAsync(userAnswer);
                            await _uow.Commit(cancellationToken);
                        }
                        


                    }
                }
            }
            Double finalScore = Math.Round(totalScore, 2);


            scoreEntity.ScoreValue = (decimal)finalScore;

             _uow.ScoreRepository.Update(scoreEntity);
            await _uow.Commit(cancellationToken);

            return _mapper.Map<ScoreDto>(result);
        }


        public async Task<double> CheckAnswerEssay(int questionId, string userAnswerText, double pointsPerQuestion, CancellationToken cancellationToken)
        {
            var question = await _uow.QuestionRepository.FirstOrDefaultAsync(x => x.QuestionId == questionId, cancellationToken, include: x => x.Include(a => a.Answers));
            if (question == null) { return 0.0; }
            var answerText = question.Answers.FirstOrDefault()?.Text;
            if (answerText == null) { return 0.0; }
            var similarityScore = await _similarityService.GetSimilarityScoreAsync(new CompareAnswersRequest { SampleAnswer = answerText, UserAnswer = userAnswerText });
            double finalScore = Math.Round(similarityScore * pointsPerQuestion, 2) > 0 ? Math.Round(similarityScore * pointsPerQuestion, 2) : 0.00;
            return finalScore;
        }
        public async Task<bool> CheckAnswerCorrect(int questionId, List<int> answerId, CancellationToken cancellationToken)
        {
            if (questionId <= 0)
                return false;

            if (answerId == null || !answerId.Any() || answerId.Any(a => a <= 0))
                return false;

            var question = await _uow.QuestionRepository.FirstOrDefaultAsync(
                x => x.QuestionId == questionId,
                cancellationToken,
                include: i => i.Include(a => a.Answers));

            if (question == null)
                return false;

            var answerCorrectIds = question.Answers
                                            .Where(a => a.IsCorrect)
                                            .Select(a => a.AnswerId)
                                            .ToList();

            if (answerCorrectIds.Count != answerId.Count)
                return false;

            return !answerCorrectIds.Except(answerId).Any();
        }


        public async Task<List<ScoreDto>> GetScoreByUserId(int userId, int? examId, CancellationToken cancellationToken)
        {
            var query = await _uow.ScoreRepository.WhereAsync(x => x.UserId == userId);

            if (examId.HasValue)
            {
                query = query.Where(x => x.ExamId == examId.Value);
            }

            if (!query.Any())
            {
                throw new KeyNotFoundException("Score not found.");
            }

            return _mapper.Map<List<ScoreDto>>(query);
        }
    }
}
