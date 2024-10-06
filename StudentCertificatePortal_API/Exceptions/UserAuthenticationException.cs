namespace StudentCertificatePortal_API.Exceptions
{
    public class UserAuthenticationException: Exception
    {
        public UserAuthenticationException(string message) : base(message) { }
    }
}
