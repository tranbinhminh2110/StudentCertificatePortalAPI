using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using StudentCertificatePortal_API.Commons;
using StudentCertificatePortal_API.DTOs;
using StudentCertificatePortal_API.Services.Interface;
using StudentCertificatePortal_API.Utils;

namespace StudentCertificatePortal_API.Controllers
{
    public class NotificationController : ApiControllerBase
    {
        private readonly INotificationService _service;
        private readonly IMapper _mapper;
        private readonly IHubContext<NotificationHub> _hubContext;

        public NotificationController(INotificationService service, IMapper mapper, IHubContext<NotificationHub> hubContext)
        {
            _service = service;
            _mapper = mapper;
            _hubContext = hubContext;
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
        [HttpPut("IsReadByNotificationId/{notificationId:int}")]
        public async Task<ActionResult<Result<List<NotificationDto>>>> UpdateIsReadById(int notificationId)
        {
            var result = await _service.UpdateIsReadByIdAsync(notificationId, new CancellationToken());
            return Ok(Result<List<NotificationDto>>.Succeed(result));
        }
        [HttpPut("IsRead")]
        public async Task<ActionResult<Result<List<NotificationDto>>>> UpdateNotificationIsRead(string role)
        {
            var result = await _service.UpdateNotificationIsReadAsync(role, new CancellationToken());
            return Ok(Result<List<NotificationDto>>.Succeed(result));
        }
        [HttpPut("RecordViolations/{notificationId:int}")]
        public async Task<ActionResult<Result<List<NotificationDto>>>> UpdateAdminIsRead(int notificationId)
        {
            var result = await _service.UpdateAdminIsReadAsync(notificationId, new CancellationToken());
            return Ok(Result<List<NotificationDto>>.Succeed(result));
        }

        [HttpPost("send")]
        public async Task<IActionResult> SendNotification(string message)
        {
            await _hubContext.Clients.All.SendAsync("ReceiveNotification", message);
            return Ok(new { Message = "Notification sent successfully" });
        }
        [HttpGet("Student/{userId:int}")]
        public async Task<ActionResult<Result<List<NotificationDto>>>> GetNotificationStudent(int userId)
        {
            var result = await _service.GetNotificationByStudentAsync(userId, new CancellationToken());
            return Ok(Result<List<NotificationDto>>.Succeed(result));
        }
    }
}
