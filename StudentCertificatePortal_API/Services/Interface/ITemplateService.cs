using StudentCertificatePortal_API.Contracts.Responses;

namespace StudentCertificatePortal_API.Services.Interface
{
    public interface ITemplateService
    {
        byte[] GenerateExamTemplate();
        Task<List<DuplicateQuestionInfoResponse>> AddQuestionsFromExcelAsync(int examId, Stream fileStream, CancellationToken cancellationToken);
    }
}
