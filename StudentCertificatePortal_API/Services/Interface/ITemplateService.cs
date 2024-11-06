namespace StudentCertificatePortal_API.Services.Interface
{
    public interface ITemplateService
    {
        byte[] GenerateExamTemplate();
        Task AddQuestionsFromExcelAsync(Stream fileStream, CancellationToken cancellationToken);
    }
}
