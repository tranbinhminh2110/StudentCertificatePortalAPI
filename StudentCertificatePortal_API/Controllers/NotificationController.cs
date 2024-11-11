using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using StudentCertificatePortal_API.Commons;
using StudentCertificatePortal_API.DTOs;
using StudentCertificatePortal_API.Services.Interface;

namespace StudentCertificatePortal_API.Controllers
{
    public class NotificationController : ApiControllerBase
    {
        private readonly INotificationService _service;
        private readonly IMapper _mapper;

        public NotificationController(INotificationService service, IMapper mapper)
        {
            _service = service;
            _mapper = mapper;
        }

        [HttpGet]
        public async Task<ActionResult<Result<List<NotificationDto>>>> GetAllNotification()
        {
            var result = await _service.GetAll();
            return Ok(Result<List<NotificationDto>>.Succeed(result));
        }
        [HttpGet("{notificationId:int}")]
        public async Task<ActionResult<Result<NotificationDto>>> GetNotificationById([FromRoute] int notificationId)
        {
            var result = await _service.GetNotificationByIdAsync(notificationId, new CancellationToken());
            return Ok(Result<NotificationDto>.Succeed(result));
        }
        [HttpGet("{role}")]
        public async Task<ActionResult<Result<List<NotificationDto>>>> GetNotificationByRole(string role)
        {
            var result = await _service.GetNotificationByRoleAsync(role, new CancellationToken());
            return Ok(Result<List<NotificationDto>>.Succeed(result));
        }
        [HttpDelete("{notificationId:int}")]
        public async Task<ActionResult<Result<NotificationDto>>> DeleteNotificationById([FromRoute] int notificationId)
        {
            var result = await _service.DeleteNotificationAsync(notificationId, new CancellationToken());
            return Ok(Result<NotificationDto>.Succeed(result));
        }
    }
}
