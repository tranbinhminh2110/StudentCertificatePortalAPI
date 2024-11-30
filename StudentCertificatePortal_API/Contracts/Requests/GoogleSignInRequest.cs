using System.ComponentModel.DataAnnotations;

namespace StudentCertificatePortal_API.Contracts.Requests
{
    public class GoogleSignInRequest
    {
        public string IdToken { get; set; }
    }
}
