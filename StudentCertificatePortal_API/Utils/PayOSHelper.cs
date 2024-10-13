using System.Security.Cryptography;
using System.Text;

namespace StudentCertificatePortal_API.Utils
{
    public class PayOSHelper
    {
        public static string CreateSignature(string amount, string cancelUrl, string description, string orderCode, string returnUrl, string checksumKey)
        {

            var data = $"amount={amount}&cancelUrl={cancelUrl}&description={description}&orderCode={orderCode}&returnUrl={returnUrl}";


            using (var hmacsha256 = new HMACSHA256(Encoding.UTF8.GetBytes(checksumKey)))
            {
                var hashBytes = hmacsha256.ComputeHash(Encoding.UTF8.GetBytes(data));
                var signature = BitConverter.ToString(hashBytes).Replace("-", "").ToLower();
                return signature;
            }
        }
    }
}
