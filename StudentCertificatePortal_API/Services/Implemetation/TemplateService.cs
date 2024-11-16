using OfficeOpenXml;
using StudentCertificatePortal_API.Contracts.Requests;
using StudentCertificatePortal_API.Services.Interface;
using StudentCertificatePortal_Data.Models;
using StudentCertificatePortal_Repository.Interface;

namespace StudentCertificatePortal_API.Services.Implemetation
{
    public class TemplateService : ITemplateService
    {
        private readonly IUnitOfWork _uow;

        public TemplateService(IUnitOfWork uow)
        {
            _uow = uow;
        }

        public async Task AddQuestionsFromExcelAsync(int examId, Stream fileStream, CancellationToken cancellationToken)
        {
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;

            using (var package = new ExcelPackage(fileStream))
            {
                var worksheet = package.Workbook.Worksheets[0];
                int rowCount = worksheet.Dimension.Rows;

                var exam = await _uow.SimulationExamRepository.FirstOrDefaultAsync(x => x.ExamId == examId);
                if (exam == null) throw new KeyNotFoundException("Simulation not found!");

                for (int row = 2; row <= rowCount; row++)
                {
                    var questionText = worksheet.Cells[row, 1].Value?.ToString();

                    if (string.IsNullOrWhiteSpace(questionText))
                        continue;

                    var question = new Question
                    {
                        ExamId = exam.ExamId,
                        QuestionText = questionText
                    };

                    

                    var check = new CreateQuestionRequest()
                    {
                        QuestionName = question.QuestionText,
                    };
                    for (int col = 2; col <= 9; col += 2)
                    {
                        var answerText = worksheet.Cells[row, col].Value?.ToString();
                        var isCorrectText = worksheet.Cells[row, col + 1].Value?.ToString();
                        var isCorrect = bool.TryParse(isCorrectText, out var parsedIsCorrect) && parsedIsCorrect;

                        check.Answers.Add(new AnswerRequest() { Text = answerText, IsCorrect = isCorrect });
                    }

                    bool isDuplicate = await IsDuplicateQuestionAsync(check, cancellationToken);
                    if (isDuplicate)
                    {
                        continue;
                    }
                    else
                    {
                        var questionResult = await _uow.QuestionRepository.AddAsync(question, cancellationToken);
                        await _uow.Commit(cancellationToken);
                        for (int col = 2; col <= 9; col += 2)
                        {
                            var answerText = worksheet.Cells[row, col].Value?.ToString();
                            var isCorrectText = worksheet.Cells[row, col + 1].Value?.ToString();
                            var isCorrect = bool.TryParse(isCorrectText, out var parsedIsCorrect) && parsedIsCorrect;

                            if (!string.IsNullOrWhiteSpace(answerText))
                            {
                                var entity = new Answer()
                                {
                                    QuestionId = questionResult.QuestionId,
                                    Text = answerText,
                                    IsCorrect = isCorrect
                                };
                                await _uow.AnswerRepository.AddAsync(entity, cancellationToken);
                                await _uow.Commit(cancellationToken);
                            }
                        }
                    }
                }
            }
        }
        public async Task<bool> IsDuplicateQuestionAsync(CreateQuestionRequest request, CancellationToken cancellationToken)
        {
            var simulation = await _uow.SimulationExamRepository
                .FirstOrDefaultAsync(x => x.ExamId == request.ExamId, cancellationToken);

            if (simulation == null)
            {
                throw new Exception("Simulation not found. Question creation requires a valid CertId.");
            }

            var existingQuestion = await _uow.QuestionRepository
                .WhereAsync(q => q.ExamId == request.ExamId && q.QuestionText != null, cancellationToken);
            var matchingQuestion = existingQuestion
                .Where(q => q.QuestionText.Trim().ToLower() == request.QuestionName.Trim().ToLower())
                .ToList();

            if (matchingQuestion.Any())
            {
                var existingAnswers = matchingQuestion.SelectMany(q => q.Answers).ToList();

                var isDuplicateAnswers = request.Answers.All(r =>
                    existingAnswers.Any(a =>
                        a.Text != null &&
                        a.Text.Trim().ToLower() == r.Text.Trim().ToLower() &&
                        a.IsCorrect == r.IsCorrect));

                if (isDuplicateAnswers)
                {
                    return true;
                }
            }

            return false;
        }
        public byte[] GenerateExamTemplate()
        {
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
            using (var package = new ExcelPackage())
            {
                var examSheet = package.Workbook.Worksheets.Add("Exam Template");
                examSheet.Cells["A1"].Value = "Question";
                examSheet.Cells["B1"].Value = "Answer 1";
                examSheet.Cells["C1"].Value = "IsCorrect (1)";
                examSheet.Cells["D1"].Value = "Answer 2";
                examSheet.Cells["E1"].Value = "IsCorrect (2)";
                examSheet.Cells["F1"].Value = "Answer 3";
                examSheet.Cells["G1"].Value = "IsCorrect (3)";
                examSheet.Cells["H1"].Value = "Answer 4";
                examSheet.Cells["I1"].Value = "IsCorrect (4)";

                examSheet.Cells["A1:N1"].Style.Font.Bold = true;
                examSheet.Cells["A:I"].AutoFitColumns();

                return package.GetAsByteArray();
            }
        }


    }
}
