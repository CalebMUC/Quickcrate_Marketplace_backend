using Minimart_Api.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Data;
using System.Data.SqlClient;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Minimart_Api.DTOS.Authorization;

namespace Authentication_and_Authorization_Api.Core
{
    public class CoreLibraries
    {
        private readonly IConfiguration _config;

        public CoreLibraries(IConfiguration config)
        {
            _config = config;
        }

        //Generate Refresh Token
        public static RefreshTokens GenerateRefreshToken(string userID)
        {
            var refreshToken = new RefreshTokens
            {
                RefreshToken = Guid.NewGuid().ToString(),
                UserName = userID,
                ExpiryDate = DateTime.Now.AddDays(7)
            };

            return refreshToken;
        }

        // Generate Token for ApplicationUser (Identity)
        public string GenerateToken(ApplicationUser user)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.UTF8.GetBytes(_config["Jwt:Key"]);
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new Claim[]
                {
                    new Claim(ClaimTypes.NameIdentifier, user.Id),
                    new Claim(ClaimTypes.Name, user.DisplayName ?? user.UserName ?? ""),
                    new Claim(ClaimTypes.Email, user.Email ?? ""),
                    new Claim("userId", user.Id)
                }),
                Expires = DateTime.UtcNow.AddHours(1),
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature),
                Issuer = _config["Jwt:Issuer"],
                Audience = _config["Jwt:Audience"]
            };

            var token = tokenHandler.CreateToken(tokenDescriptor);
            var tokenString = tokenHandler.WriteToken(token);

            return tokenString;
        }

        public static CookieOptions SetRefreshToken(RefreshTokens newrefreshToken)
        {
            var cookieOptions = new CookieOptions
            {
                HttpOnly = true,
                Expires = newrefreshToken.ExpiryDate
            };

            return cookieOptions;
        }
    }
}
