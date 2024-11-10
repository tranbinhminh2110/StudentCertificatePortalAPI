using Microsoft.AspNetCore.Identity.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using StudentCertificatePortal_API.Contracts.Requests;
using StudentCertificatePortal_API.Services.Implemetation;
using StudentCertificatePortal_API.Services.Interface;
using StudentCertificatePortal_API.Utils;
using StudentCertificatePortal_API.Commons;
using StudentCertificatePortal_API.DTOs;

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
            await _redisService.SaveResetTokenAsync(user.UserId, token, TimeSpan.FromMinutes(10));

            bool emailSent = await _emailService.SendEmailAsync(user.Email, "Password change code", $"Your code is: {token}");

            if (!emailSent)
            {
                return StatusCode(500, "Email could not be sent. Please try again.");
            }

            return Ok("Password change code has been sent via email.");
        }



        [HttpPost("reset-password")]
        public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordRequest request)
        {
            var user = await _service.FindByEmailAsync(request.Email);
            if (user == null)
            {
                return BadRequest("Email does not exist.");
            }

            var storedToken = await _redisService.GetResetTokenAsync(user.UserId);

            if (storedToken == null || storedToken != request.ResetCode)
            {
                return BadRequest("Code is invalid or expired.");
            }

            var result = await _service.ResetPasswordAsync(user.Email, request.NewPassword);
            if (!result.Succeeded)
            {
                return BadRequest("Password change failed.");
            }
            await _redisService.DeleteResetTokenAsync(user.UserId);

            return Ok("Password changed successfully.");
        }

        [HttpPost("change-password/{userId:int}")]
        public async Task<IActionResult> ChangePassword([FromRoute] int userId, [FromBody] ChangePasswordRequest request)
        {
            var result = await _service.ChangePasswordAsync(userId, request, new CancellationToken());
            if (result is null) return BadRequest("Password change failed.");
            return Ok(Result<string>.Succeed("Password changed successfully."));
        }
    }
}
