using Microsoft.EntityFrameworkCore;
using StudentCertificatePortal_API.Services.Interface;
using StudentCertificatePortal_Data.Models;
using StudentCertificatePortal_Repository.Implementation;
using StudentCertificatePortal_Repository.Interface;

namespace StudentCertificatePortal_API.Services.Implemetation
{
    public class PermissionService<T> : IPermissionService<T> where T : class
    {
        private readonly IUnitOfWork _uow;
        private readonly Dictionary<Type, string> _permissionMappings = new()
        {
            { typeof(Certification), "Permission" },
            {typeof(SimulationExam), "ExamPermission" }
        };

        private readonly Dictionary<Type, string> _idFieldMapping = new Dictionary<Type, string>()
        {
            {typeof(Certification) , "CertId"},
            {typeof(SimulationExam) , "ExamId"}
        };
        public PermissionService(IUnitOfWork uow)
        {
            _uow = uow;
        }
        public async Task<bool> UpdatePermissionAsync(int id, Enums.EnumPermission newPermission, CancellationToken cancellationToken)
        {
            IBaseRepository<T> repository = GetRepository();

            if (repository == null) return false;
            
            if(_idFieldMapping.TryGetValue(typeof(T), out string idField)){
                var entity = await repository.FirstOrDefaultAsync(e => EF.Property<int>(e, idField) == id);
                if (entity == null) return false;

                if(_permissionMappings.TryGetValue(typeof(T), out string permissionField)) {
                    var permissionProperty = typeof(T).GetProperty(permissionField);
                    if(permissionProperty != null && permissionProperty.CanWrite)
                    {
                        permissionProperty.SetValue(entity, newPermission.ToString());
                    }
                    else
                    {
                        return false;
                    }
                }
                else
                {
                    return false;
                }

                 repository.Update(entity);
                await _uow.Commit(cancellationToken); 
                return true;
            }
            return false;
            
        }

        private IBaseRepository<T> GetRepository()
        {
            return typeof(T) switch
            {
                Type when typeof(T) == typeof(Certification) => _uow.CertificationRepository as IBaseRepository<T>,
                Type when typeof(T) == typeof(SimulationExam) => _uow.SimulationExamRepository as IBaseRepository<T>,
                _ => null
            };
        }
    }
}
