using MailKit.Security;
using MimeKit;
using StudentCertificatePortal_API.Services.Interface;

namespace StudentCertificatePortal_API.Services.Implemetation
{
    public class EmailService : IEmailService
    {
        private readonly string _fromEmail;
        private readonly string _appPassword;

        public EmailService(IConfiguration configuration)
        {
            _fromEmail = "unicert79@gmail.com"; // Email người gửi
            _appPassword = configuration.GetSection("Authentication:Google")["AppPassword"]; // Mật khẩu ứng dụng từ Google
        }

        public async Task<bool> SendEmailAsync(string email, string subject, string message)
        {
            // Tạo email bằng MimeKit
            var mimeMessage = new MimeMessage();
            mimeMessage.From.Add(new MailboxAddress("Sender Name", _fromEmail));
            mimeMessage.To.Add(new MailboxAddress("Recipient", email));
            mimeMessage.Subject = subject;

            // Thiết lập nội dung email
            var bodyBuilder = new BodyBuilder
            {
                TextBody = message
            };
            mimeMessage.Body = bodyBuilder.ToMessageBody();

            try
            {
                // Sử dụng MailKit SmtpClient để gửi email
                using (var client = new MailKit.Net.Smtp.SmtpClient())
                {
                    client.Connect("smtp.gmail.com", 587, SecureSocketOptions.StartTls);

                    // Xác thực bằng mật khẩu ứng dụng
                    client.Authenticate(_fromEmail, _appPassword);

                    // Gửi email
                    await client.SendAsync(mimeMessage);
                    client.Disconnect(true);
                }
                return true; // Email đã được gửi thành công
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error sending email: " + ex.Message);
                return false; // Gửi email thất bại
            }
        }
    }
}