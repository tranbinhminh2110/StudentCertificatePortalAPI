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

        public ScoreService(IUnitOfWork uow, IMapper mapper)
        {
            _uow = uow;
            _mapper = mapper;
        }
        public async Task<ScoreDto> Scoring(UserAnswerRequest request, CancellationToken cancellationToken)
        {
            int countQuestionCorrect = 0;
            var exam = await _uow.SimulationExamRepository.FirstOrDefaultAsync(x => x.ExamId == request.ExamId);
            var user = await _uow.UserRepository.FirstOrDefaultAsync(x => x.UserId == request.UserId);
            if (user is null) throw new KeyNotFoundException("User not found!");
            if (exam == null) throw new KeyNotFoundException("Simulation not found!");

            var enroll = await _uow.ExamEnrollmentRepository.WhereAsync(x => x.UserId == user.UserId && x.StudentOfExams.Any(x => x.ExamId == exam.ExamId));
            if (!enroll.Any())
            {
                throw new InvalidOperationException("User is not enrolled in this exam.");
            }
            var numberQuestion = exam.QuestionCount ?? 0;
            foreach (var model in request.QuestionRequests)
            {
                bool checkQuestion = await CheckAnswerCorrect(model.QuestionId, model.UserAnswerId, cancellationToken);
                if (checkQuestion)
                {
                    countQuestionCorrect++;
                }
            }
            float finalScore = countQuestionCorrect * (100f / numberQuestion);

            var scoreEntity = new Score()
            {
                UserId = request.UserId,
                ExamId = request.ExamId,
                ScoreValue = (decimal)finalScore,
            };

            var result = await _uow.ScoreRepository.AddAsync(scoreEntity);
            await _uow.Commit(cancellationToken);

            return _mapper.Map<ScoreDto>(result);
        }

        public async Task<bool> CheckAnswerCorrect(int questionId, List<int> answerId, CancellationToken cancellationToken)
        {
            var question = await _uow.QuestionRepository.FirstOrDefaultAsync(x => x.QuestionId == questionId,
                cancellationToken,
                include: i => i.Include(a => a.Answers));

            if (question != null)
            {
                List<int> answerCorrectIds = new List<int>();
                foreach (var answer in question.Answers)
                {
                    if (answer.IsCorrect)
                    {
                        answerCorrectIds.Add(answer.AnswerId);
                    }
                }
                if (answerCorrectIds.Count == answerId.Count)
                {
                    if (answerId.Count == 1)
                    {
                        if (answerId[0] == answerCorrectIds[0])
                        {
                            return true;
                        }
                        return false;
                    }
                    else if (answerId.Count > 1)
                    {
                        int count = 0;
                        for (int i = 0; i < answerId.Count; i++)
                        {
                            foreach (var ansCorrect in answerCorrectIds)
                            {
                                if (answerId[0] == ansCorrect)
                                {
                                    count++;
                                }
                            }
                        }
                        if (count == answerCorrectIds.Count)
                        {
                            return true;
                        }
                        else
                        {
                            return false;
                        }
                    }
                }
                return false;
            }
            return false;
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
