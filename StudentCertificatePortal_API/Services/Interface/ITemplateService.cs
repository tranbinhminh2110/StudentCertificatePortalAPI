namespace StudentCertificatePortal_API.Services.Interface
{
    public interface ITemplateService
    {
        byte[] GenerateExamTemplate();
        Task AddQuestionsFromExcelAsync(int examId, Stream fileStream, CancellationToken cancellationToken);
    }
}
