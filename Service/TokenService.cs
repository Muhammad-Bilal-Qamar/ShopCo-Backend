using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using ShopCoAPI.Models;
using ShopCoAPI.Settings;

namespace ShopCoAPI.Services
{
    public class TokenService : ITokenService
    {
        private readonly JwtSettings _JwtSettings;

        public TokenService(IOptions<JwtSettings> jwtSettings)
        {
            _JwtSettings = jwtSettings.Value;
        }

        public string GenerateToken(Users user)
        {
            var claims = new List<Claim>
            {
                new(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
                new(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new(ClaimTypes.Name, user.Name),
                new(ClaimTypes.Email, user.Email),
                new(ClaimTypes.Role, user.Role),
                new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
            };

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_JwtSettings.Key));
            // use HMAC-SHA256 to match typical key size and validation expectations
            var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: _JwtSettings.Issuer,
                audience: _JwtSettings.Audience,
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(_JwtSettings.ExpiryMinutes),
                signingCredentials: credentials
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        public string GenerateRefreshToken()
        {
            var randomNumber = new byte[32];
            using var rng = RandomNumberGenerator.Create();
            rng.GetBytes(randomNumber);
            return Convert.ToBase64String(randomNumber);
        }
    }
}
