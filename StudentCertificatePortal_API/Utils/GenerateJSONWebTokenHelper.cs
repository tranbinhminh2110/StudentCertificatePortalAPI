using Microsoft.IdentityModel.Tokens;
using StudentCertificatePortal_API.DTOs;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace StudentCertificatePortal_API.Utils
{
    public class GenerateJSONWebTokenHelper
    {
        private readonly IConfiguration _configuration;
        public GenerateJSONWebTokenHelper(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public string GenerateJSONWebToken(UserDto user)
        {
            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]));
            var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

            var claims = GenerateClaims(user);

            var token = new JwtSecurityToken(_configuration["Jwt:Issuer"],
              _configuration["Jwt:Issuer"],
              claims,
              expires: DateTime.Now.AddMinutes(120),
              signingCredentials: credentials);

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        private static IEnumerable<Claim> GenerateClaims(UserDto user)
        {
            var claims = new List<Claim>();

            // Add email as Name claim
            claims.Add(new Claim(ClaimTypes.Name, user.Email));

            // Add role
            claims.Add(new Claim(ClaimTypes.Role, user.Role));

            return claims;
        }
    }
}
