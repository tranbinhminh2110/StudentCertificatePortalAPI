using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using StudentCertificatePortal_API.Services.Interface;
using StudentCertificatePortal_API.Utils;
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
        private readonly Dictionary<Type, string> _imageFieldMapping = new Dictionary<Type, string>()
    {
        { typeof(Certification), "CertImage" }, 
        { typeof(SimulationExam), "ExamImage" }  
    };


        private readonly IHubContext<NotificationHub> _hubContext;

        private readonly INotificationService _notificationService;
        public PermissionService(IUnitOfWork uow, IHubContext<NotificationHub> hubContext, INotificationService notificationService)
        {
            _uow = uow;
            _hubContext = hubContext;
            _notificationService = notificationService;
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
                string entityName = null;
                if (typeof(T) == typeof(Certification))
                {
                    var nameProperty = typeof(Certification).GetProperty("CertName");
                    entityName = nameProperty?.GetValue(entity)?.ToString();
                }
                else if (typeof(T) == typeof(SimulationExam))
                {
                    var nameProperty = typeof(SimulationExam).GetProperty("ExamName");
                    entityName = nameProperty?.GetValue(entity)?.ToString();
                }
                string imageUrl = null;
                if (_imageFieldMapping.TryGetValue(typeof(T), out string imageField))
                {
                    var imageProperty = typeof(T).GetProperty(imageField);
                    if (imageProperty != null)
                    {
                        imageUrl = imageProperty.GetValue(entity)?.ToString();
                    }
                }

                var notification = new Notification
                {
                    NotificationName = $"{typeof(T).Name} Permission Update",
                    NotificationDescription = $"{entityName} has been {newPermission}.",
                    NotificationImage = imageUrl,
                    CreationDate = DateTime.UtcNow,
                    Role = "Staff",
                    IsRead = false,
                    NotificationType = typeof(T).Name,
                    NotificationTypeId = id,
                    
                };

                await _uow.NotificationRepository.AddAsync(notification);
                await _uow.Commit(cancellationToken);

                var notifications = await _notificationService.GetNotificationByRoleAsync("staff", new CancellationToken());
                await _hubContext.Clients.All.SendAsync("ReceiveNotification", notifications);
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
