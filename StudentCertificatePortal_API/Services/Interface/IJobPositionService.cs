using StudentCertificatePortal_API.Contracts.Requests;
using StudentCertificatePortal_API.DTOs;

namespace StudentCertificatePortal_API.Services.Interface
{
    public interface IJobPositionService
    {
        Task<JobPositionDto> CreateJobPositionAsync(CreateJobPositionRequest request, CancellationToken cancellationToken);
        Task<List<JobPositionDto>> GetAll();
        Task<JobPositionDto> GetJobPositionByIdAsync(int jobPositionId, CancellationToken cancellationToken);
        Task <JobPositionDto> UpdateJobPositionAsync(int jobPositionId, UpdateJobPositionRequest request, CancellationToken cancellationToken = default);
        Task<JobPositionDto> DeleteJobPositionAsync(int jobPositionId, CancellationToken cancellationToken);
        Task<List<JobPositionDto>> GetJobPositionByNameAsync(string jobPositionName, CancellationToken cancellationToken);
    }
}
