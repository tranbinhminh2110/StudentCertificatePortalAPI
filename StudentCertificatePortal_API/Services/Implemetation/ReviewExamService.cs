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
                throw new KeyNotFoundException("No answers found for this user in the given exam and attempt.");

            var score = await _uow.ScoreRepository.FirstOrDefaultAsync(
                x => x.ScoreId == scoreId && x.UserId == userId && x.ExamId == examId,
                cancellationToken);

            if (score == null)
                throw new KeyNotFoundException("Score record not found for this attempt.");

            var reviewDto = new ExamReviewDto()
            {
                ExamId = examId,
                UserId = userId,
                TotalScore = score.ScoreValue, // Ensure decimal value
                Questions = new List<QuestionReviewDto>()
            };

            foreach (var userAnswer in userAnswers)
            {
                var question = await _uow.QuestionRepository.FirstOrDefaultAsync(q => q.QuestionId == userAnswer.QuestionId, cancellationToken);

                // Ensure that the question is not null before proceeding
                if (question == null)
                {
                    throw new KeyNotFoundException($"Question with ID {userAnswer.QuestionId} not found.");
                }

                var systemAnswers = new List<AnswerDto>();

                // Check if the question is Choice or MultipleChoice
                if (question.QuestionType == "Choice" || question.QuestionType == "MultipleChoice")
                {
                    systemAnswers = (await _uow.AnswerRepository.WhereAsync(a => a.QuestionId == question.QuestionId, cancellationToken))
                        .Select(a => new AnswerDto
                        {
                            AnswerId = a.AnswerId,
                            IsCorrect = a.IsCorrect,
                            Text = a.Text,
                        }).ToList();
                }

                // For "Choice" or "MultipleChoice", collect multiple answers selected by the user
                List<int> userAnswerIds = new List<int>();
                if (userAnswer.QuestionType == "Choice" || userAnswer.QuestionType == "MultipleChoice")
                {
                    userAnswerIds = userAnswers
                        .Where(x => x.QuestionId == userAnswer.QuestionId)  // Collect all answers for this question
                        .Select(x => x.AnswerId ?? 0)
                        .ToList();
                }
                else if (userAnswer.QuestionType == "Essay")
                {
                    userAnswerIds = new List<int>();  // No options for Essay type
                }
                else
                {
                    userAnswerIds = new List<int> { userAnswer.AnswerId ?? 0 };  // For other types, store the answer id
                }

                // Add the QuestionReviewDto to the result list
                reviewDto.Questions.Add(new QuestionReviewDto
                {
                    QuestionId = userAnswer.QuestionId ?? 0,
                    QuestionType = userAnswer.QuestionType,
                    UserAnswers = userAnswerIds,  // Store multiple answer IDs for Choice/MultipleChoice questions
                    SystemAnswers = systemAnswers,  // Only for Choice/MultipleChoice questions
                    IsCorrectQuestion = userAnswer.IsCorrect,
                    ScoreValue = userAnswer.ScoreValue ?? 0,
                    SubmittedAt = userAnswer.SubmittedAt
                });
            }

            return reviewDto;
        }



    }
}
