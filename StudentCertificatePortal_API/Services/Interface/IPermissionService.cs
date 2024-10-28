using StudentCertificatePortal_Data.Models;
using StudentCertificatePortal_Repository.Implementation;
using StudentCertificatePortal_Repository.Interface;

namespace StudentCertificatePortal_API.Services.Interface
{
    public interface IPermissionService<T> where T : class
    {
        Task<bool> UpdatePermissionAsync(int id, Enums.EnumPermission newPermission, CancellationToken cancellationToken);
    }

}
