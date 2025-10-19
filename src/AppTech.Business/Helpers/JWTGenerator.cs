using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using AppTech.Core.Entities.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;

namespace AppTech.Business.Helpers
{
    public static class JwtGenerator
    {
        public static string GenerateToken(User user, string userRole, IConfiguration configuration)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(configuration["JwtConfiguration:SecretKey"]);

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new Claim[]
                {
                    new Claim("id", user.Id.ToString()),
                    new Claim("role", userRole),
                }),
                Expires = DateTime.UtcNow.AddDays(30),
                Issuer = configuration["JwtConfiguration:Issuer"],
                Audience = configuration["JwtConfiguration:Audience"],
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            };

            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }
    }
}
