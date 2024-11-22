using StudentCertificatePortal_API.Contracts.Requests;
using StudentCertificatePortal_API.DTOs;

namespace StudentCertificatePortal_API.Services.Interface
{
    public interface ISimulationExamService
    {
        Task<SimulationExamDto> CreateSimulationExamAsync(CreateSimulationExamRequest request, CancellationToken cancellationToken);
        Task<List<SimulationExamDto>> GetAll();
        Task<SimulationExamDto> GetSimulationExamByIdAsync(int examId, CancellationToken cancellationToken);
        Task<List<SimulationExamDto>> GetSimulationExamByCertIdAsync(int certId, CancellationToken cancellationToken);
        Task<SimulationExamDto> UpdateSimulationExamAsync(int examId, UpdateSimulationExamRequest request, CancellationToken cancellationToken);
        Task<SimulationExamDto> DeleteSimulationExamAsync(int examId, CancellationToken cancellationToken);
        Task<List<SimulationExamDto>> GetSimulationExamByNameAsync(string examName, CancellationToken cancellationToken);
        Task<SimulationExamDto> UpdateExamVouchersAsync(int examId, List<int> voucherIds, CancellationToken cancellationToken);
    }
}
