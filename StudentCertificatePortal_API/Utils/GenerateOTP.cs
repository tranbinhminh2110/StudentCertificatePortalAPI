namespace StudentCertificatePortal_API.Utils
{
    public class GenerateOTP
    {
        public string GenerateNumericToken(int length = 6)
        {
            var random = new Random();
            string token = random.Next(0, 999999).ToString("D6");
            return token;
        }
    }
}
