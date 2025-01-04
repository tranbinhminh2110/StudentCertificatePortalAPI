using StudentCertificatePortal_API.DTOs;
using StudentCertificatePortal_API.Services.Interface;
using StudentCertificatePortal_Repository.Interface;

namespace StudentCertificatePortal_API.Services.Implemetation
{
    public class ReviewExamService : IReviewExamService
    {
        private readonly IUnitOfWork _uow;

        public ReviewExamService(IUnitOfWork uow)
        {
            _uow = uow;
        }

        public async Task<ExamReviewDto> GetExamReviewAsync(int examId, int userId, int scoreId, CancellationToken cancellationToken)
        {
            var userAnswers = await _uow.UserAnswerRepository.WhereAsync(
                x => x.ExamId == examId && x.UserId == userId && x.ScoreId == scoreId,
                cancellationToken);

            if (!userAnswers.Any())
                throw new KeyNotFoundException("No user answers found for this exam.");

            var score = await _uow.ScoreRepository.FirstOrDefaultAsync(
                x => x.ScoreId == scoreId && x.UserId == userId && x.ExamId == examId,
                cancellationToken);

            if (score == null)
                throw new KeyNotFoundException("Score record not found for this attempt.");


            var questionIds = userAnswers.Select(x => x.QuestionId).Distinct();

            var questions = await _uow.QuestionRepository.WhereAsync(
                q => questionIds.Contains(q.QuestionId),
                cancellationToken);

            if (!questions.Any())
                throw new KeyNotFoundException("Questions not found for this exam.");

            var reviewDto = new ExamReviewDto
            {
                ExamId = examId,
                UserId = userId,
                TotalScore = score.ScoreValue ?? 0,
                Questions = new List<QuestionReviewDto>()
            };

            foreach (var question in questions)
            {
                var systemAnswers = new List<AnswerDto>();
                if (question.QuestionType.Equals("Choice", StringComparison.OrdinalIgnoreCase))
                {
                    systemAnswers = (await _uow.AnswerRepository.WhereAsync(
                        a => a.QuestionId == question.QuestionId,
                        cancellationToken))
                        .Select(a => new AnswerDto
                        {
                            AnswerId = a.AnswerId,
                            IsCorrect = a.IsCorrect,
                            Text = a.Text
                        })
                        .ToList();
                }

                var userAnswersForQuestion = userAnswers.Where(x => x.QuestionId == question.QuestionId).ToList();

                var userAnswerContent = question.QuestionType.Equals("Essay", StringComparison.OrdinalIgnoreCase)
                    ? userAnswersForQuestion.FirstOrDefault()?.AnswerContent ?? "No Answer"
                    : null;

                var userAnswerIds = question.QuestionType.Equals("Choice", StringComparison.OrdinalIgnoreCase)
                    ? userAnswersForQuestion
                        .Where(x => x.AnswerId.HasValue && x.AnswerId.Value != 0)
                        .Select(x => x.AnswerId.Value)
                        .ToList()
                    : null;

                var isCorrectQuestion = question.QuestionType.Equals("Choice", StringComparison.OrdinalIgnoreCase) &&
                                        userAnswersForQuestion.Any(x => x.AnswerId.HasValue && x.AnswerId.Value != 0) &&
                                        userAnswersForQuestion.All(x => x.IsCorrect);

                var scoreValue = userAnswersForQuestion.FirstOrDefault()?.ScoreValue ?? 0;
                var submittedAt = userAnswersForQuestion.FirstOrDefault()?.SubmittedAt ?? DateTime.UtcNow;

                reviewDto.Questions.Add(new QuestionReviewDto
                {
                    QuestionId = question.QuestionId,
                    QuestionName = question.QuestionText,
                    QuestionType = question.QuestionType,
                    UserAnswersForChoice = question.QuestionType.Equals("Choice", StringComparison.OrdinalIgnoreCase) ? userAnswerIds : null,
                    UserAnswerContentForEssay = question.QuestionType.Equals("Essay", StringComparison.OrdinalIgnoreCase) ? userAnswerContent : null,
                    SystemAnswers = question.QuestionType.Equals("Essay", StringComparison.OrdinalIgnoreCase) ? new List<AnswerDto>() : systemAnswers,
                    IsCorrectQuestion = isCorrectQuestion,
                    ScoreValue = scoreValue,
                    SubmittedAt = submittedAt
                });
            }

            return reviewDto;
        }

    }
}
