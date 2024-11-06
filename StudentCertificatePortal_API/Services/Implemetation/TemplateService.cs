using OfficeOpenXml;
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

        public async Task AddQuestionsFromExcelAsync(Stream fileStream, CancellationToken cancellationToken)
        {
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;

            using (var package = new ExcelPackage(fileStream))
            {
                var worksheet = package.Workbook.Worksheets[0];
                int rowCount = worksheet.Dimension.Rows;

                var examName = worksheet.Cells[2, 1].Value?.ToString();
                var examCode = worksheet.Cells[2, 2].Value?.ToString();
                var cert = worksheet.Cells[2, 3].Value?.ToString();
                var examDescription = worksheet.Cells[2, 4].Value?.ToString();
                var examImage = worksheet.Cells[2, 5].Value?.ToString();

                var certId = int.TryParse(cert, out var parsedCertId) ? parsedCertId : default;

                var entityExam = new SimulationExam
                {
                    ExamCode = examCode,
                    ExamName = examName,
                    ExamDescription = examDescription,
                    ExamImage = examImage,
                    CertId = certId
                };

                var addedExam = await _uow.SimulationExamRepository.AddAsync(entityExam);
                await _uow.Commit(cancellationToken);

                for (int row = 2; row <= rowCount; row++)
                {
                    var questionText = worksheet.Cells[row, 6].Value?.ToString();

                    if (string.IsNullOrWhiteSpace(questionText))
                        continue;

                    var question = new Question
                    {
                        ExamId = addedExam.ExamId,
                        QuestionText = questionText
                    };

                    var questionResult = await _uow.QuestionRepository.AddAsync(question, cancellationToken);
                    await _uow.Commit(cancellationToken);

                    for (int col = 7; col <= 13; col += 2)
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
                                IsCorrect = isCorrect,
                                
                            };
                            await _uow.AnswerRepository.AddAsync(entity, cancellationToken);
                            await _uow.Commit(cancellationToken);
                        }
                    }
                }
            }
        }


        public byte[] GenerateExamTemplate()
        {
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
            using (var package = new ExcelPackage())
            {
                var examSheet = package.Workbook.Worksheets.Add("Exam Template");

                examSheet.Cells["A1"].Value = "Exam Name";
                examSheet.Cells["B1"].Value = "Exam Code";
                examSheet.Cells["C1"].Value = "Cert";
                examSheet.Cells["D1"].Value = "Exam Description";
                examSheet.Cells["E1"].Value = "Exam Image";
                examSheet.Cells["F1"].Value = "Question";
                examSheet.Cells["G1"].Value = "Answer 1";
                examSheet.Cells["H1"].Value = "IsCorrect (1)";
                examSheet.Cells["I1"].Value = "Answer 2";
                examSheet.Cells["J1"].Value = "IsCorrect (2)";
                examSheet.Cells["K1"].Value = "Answer 3";
                examSheet.Cells["L1"].Value = "IsCorrect (3)";
                examSheet.Cells["M1"].Value = "Answer 4";
                examSheet.Cells["N1"].Value = "IsCorrect (4)";

                examSheet.Cells["A1:N1"].Style.Font.Bold = true;

                return package.GetAsByteArray();
            }
        }
    }
}
