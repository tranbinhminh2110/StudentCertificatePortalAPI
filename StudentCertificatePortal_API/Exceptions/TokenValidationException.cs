namespace StudentCertificatePortal_API.Exceptions
{
    public class TokenValidationException: Exception
    {
        public TokenValidationException(string message) : base(message) { }
    }
}
