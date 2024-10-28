using StudentCertificatePortal_API.Contracts.Requests;
using StudentCertificatePortal_API.DTOs;
using StudentCertificatePortal_API.Enums;

namespace StudentCertificatePortal_API.Services.Interface
{
    public interface IOrganizeService
    {
        Task<OrganizeDto> CreateOrganizeAsync(CreateOrganizeRequest request, CancellationToken cancellationToken);
        Task<List<OrganizeDto>> GetAll();
        Task<OrganizeDto> GetOrganizeByIdAsync(int organizeId, CancellationToken cancellationToken);   
        Task<OrganizeDto> UpdateOrganizeAsync(int oragnizeId, UpdateOrganizeRequest request, CancellationToken cancellationToken);
        Task<OrganizeDto> DeleteOrganizeAsync(int organizeId, CancellationToken cancellationToken);
        Task<List<OrganizeDto>> GetOrganizeByNameAsync(string organizeName, CancellationToken cancellationToken);
        Task<OrganizeDto> UpdateOrganizePermissionAsync(int organizeId, EnumPermission organizePermission, CancellationToken cancellationToken);



    }
}
