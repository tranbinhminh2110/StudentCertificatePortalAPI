using Microsoft.AspNetCore.Identity.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using StudentCertificatePortal_API.Contracts.Requests;
using StudentCertificatePortal_API.Services.Implemetation;
using StudentCertificatePortal_API.Services.Interface;
using StudentCertificatePortal_API.Utils;

namespace StudentCertificatePortal_API.Controllers
{
    public class ForgetPasswordController : ApiControllerBase
    {
        private readonly IProfileService _service;
        private readonly GenerateOTP _otp;
        private readonly IRedisService _redisService;
        private readonly IEmailService _emailService;
        public ForgetPasswordController(IProfileService service, GenerateOTP otp
            , IRedisService redisService, IEmailService emailService)
        {
            _service = service;
            _otp = otp;
            _redisService = redisService;
            _emailService = emailService;
        }

        [HttpPost("forget-password")]
        public async Task<IActionResult> ForgetPassword([FromBody] ForgetPasswordRequest request)
        {
            var user = await _service.FindByEmailAsync(request.Email);
            if (user == null)
            {
                return BadRequest("Email not found.");
            }

            var token = _otp.GenerateNumericToken();

            // Lưu token vào Redis với thời gian hết hạn (ví dụ 10 phút)
            await _redisService.SaveResetTokenAsync(user.UserId, token, TimeSpan.FromMinutes(10));

            // Gửi token qua email
            bool emailSent = await _emailService.SendEmailAsync(user.Email, "Mã đổi mật khẩu", $"Mã của bạn là: {token}");

            if (!emailSent)
            {
                return StatusCode(500, "Không thể gửi email. Vui lòng thử lại.");
            }

            return Ok("Mã đổi mật khẩu đã được gửi qua email.");
        }



        [HttpPost("reset-password")]
        public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordRequest request)
        {
            var user = await _service.FindByEmailAsync(request.Email);
            if (user == null)
            {
                return BadRequest("Email không tồn tại.");
            }

            // Lấy token từ Redis
            var storedToken = await _redisService.GetResetTokenAsync(user.UserId);

            // Kiểm tra token có hợp lệ không
            if (storedToken == null || storedToken != request.ResetCode)
            {
                return BadRequest("Mã không hợp lệ hoặc đã hết hạn.");
            }

            // Đổi mật khẩu
            var result = await _service.ResetPasswordAsync(user.Email, request.NewPassword);
            if (!result.Succeeded)
            {
                return BadRequest("Đổi mật khẩu thất bại.");
            }

            // Xóa token sau khi sử dụng
            await _redisService.DeleteResetTokenAsync(user.UserId);

            return Ok("Đổi mật khẩu thành công.");
        }
    }
}
