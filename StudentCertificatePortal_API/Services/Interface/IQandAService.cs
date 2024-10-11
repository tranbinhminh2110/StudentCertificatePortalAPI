using StudentCertificatePortal_API.Contracts.Requests;
using StudentCertificatePortal_API.DTOs;

namespace StudentCertificatePortal_API.Services.Interface
{
    public interface IQandAService
    {
        Task<QandADto> CreateQandAAsync(CreateQuestionRequest request, CancellationToken cancellationToken);
        Task<List<QandADto>> GetAll();
        Task<QandADto> GetQandAByIdAsync(int questionId, CancellationToken cancellationToken);
        Task<QandADto> UpdateQandAAsync(int questionId, UpdateQuestionRequest request, CancellationToken cancellationToken);
        Task<QandADto> DeleteQandAAsync(int questionId, CancellationToken cancellationToken);
        Task<List<QandADto>> GetQandAByNameAsync(string questionName,CancellationToken cancellationToken);
    }
}
