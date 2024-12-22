﻿using StudentCertificatePortal_API.DTOs;
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
            // Lấy danh sách câu hỏi của kỳ thi
            var questions = await _uow.QuestionRepository.WhereAsync(q => q.ExamId == examId, cancellationToken);

            if (!questions.Any())
                throw new KeyNotFoundException("No questions found for this exam.");

            // Lấy danh sách câu trả lời của người dùng cho kỳ thi
            var userAnswers = await _uow.UserAnswerRepository.WhereAsync(
                x => x.ExamId == examId && x.UserId == userId && x.ScoreId == scoreId,
                cancellationToken);

            // Lấy điểm số
            var score = await _uow.ScoreRepository.FirstOrDefaultAsync(
                x => x.ScoreId == scoreId && x.UserId == userId && x.ExamId == examId,
                cancellationToken);

            if (score == null)
                throw new KeyNotFoundException("Score record not found for this attempt.");

            // Tạo ExamReviewDto
            var reviewDto = new ExamReviewDto()
            {
                ExamId = examId,
                UserId = userId,
                TotalScore = score.ScoreValue, // Tổng điểm
                Questions = new List<QuestionReviewDto>()
            };

            // Duyệt qua tất cả các câu hỏi
            foreach (var question in questions)
            {
                var questionId = question.QuestionId;

                // Lấy danh sách đáp án hệ thống (nếu không phải là Essay)
                var systemAnswers = new List<AnswerDto>();
                if (question.QuestionType == "Choice" || question.QuestionType == "MultipleChoice")
                {
                    systemAnswers = (await _uow.AnswerRepository.WhereAsync(a => a.QuestionId == questionId, cancellationToken))
                        .Select(a => new AnswerDto
                        {
                            AnswerId = a.AnswerId,
                            IsCorrect = a.IsCorrect,
                            Text = a.Text,
                        }).ToList();
                }

                // Lấy câu trả lời của người dùng cho câu hỏi này
                var userAnswersForQuestion = userAnswers.Where(x => x.QuestionId == questionId).ToList();

                // Với câu hỏi essay, lấy nội dung trả lời từ AnswerContent
                var userAnswerContent = question.QuestionType == "Essay"
    ? (string.IsNullOrEmpty(userAnswersForQuestion.FirstOrDefault()?.AnswerContent) ? "No Answer" : userAnswersForQuestion.FirstOrDefault()?.AnswerContent)
    : null;


                var userAnswerIds = userAnswersForQuestion
                    .Where(x => x.AnswerId.HasValue)
                    .Select(x => x.AnswerId.Value)
                    .ToList();

                // Xác định câu hỏi đúng hay sai
                var isCorrectQuestion = question.QuestionType != "Essay" &&
                                        userAnswersForQuestion.Any() &&
                                        userAnswersForQuestion.All(x => x.IsCorrect);

                // Điểm và thời gian nộp của người dùng
                var scoreValue = userAnswersForQuestion.FirstOrDefault()?.ScoreValue ?? 0;
                var submittedAt = userAnswersForQuestion.FirstOrDefault()?.SubmittedAt ?? DateTime.UtcNow;

                // Thêm vào danh sách QuestionReviewDto
                reviewDto.Questions.Add(new QuestionReviewDto
                {
                    QuestionId = questionId,
                    QuestionType = question.QuestionType,
                    UserAnswersForChoice = question.QuestionType == "Essay" ? null : userAnswerIds, // Với bài essay không trả UserAnswers dạng ID
                    UserAnswerContentForEssay = userAnswerContent, // Thêm nội dung trả lời của bài essay
                    SystemAnswers = question.QuestionType == "Essay" ? new List<AnswerDto>() : systemAnswers, // Không trả SystemAnswers nếu là essay
                    IsCorrectQuestion = isCorrectQuestion,
                    ScoreValue = scoreValue,
                    SubmittedAt = submittedAt
                });
            }

            return reviewDto;
        }
    }
}
