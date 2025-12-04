using System;
using System.Configuration;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;

namespace Wayfinder.API.Services
{
    /// <summary>
    /// Service for JWT token generation and validation with JWE encryption
    /// </summary>
    public class JwtService
    {
        private readonly string _secretKey;
        private readonly string _encryptionKey;
        private readonly string _issuer;
        private readonly string _audience;
        private readonly int _expirationMinutes;

        public JwtService()
        {
            _secretKey = ConfigurationManager.AppSettings["Jwt:SecretKey"] 
                ?? throw new ConfigurationErrorsException("JWT secret key not configured");
            _encryptionKey = ConfigurationManager.AppSettings["Jwe:EncryptionKey"] 
                ?? throw new ConfigurationErrorsException("JWE encryption key not configured");
            _issuer = ConfigurationManager.AppSettings["Jwt:Issuer"] ?? "Wayfinder.API";
            _audience = ConfigurationManager.AppSettings["Jwt:Audience"] ?? "Wayfinder.Client";
            _expirationMinutes = int.Parse(ConfigurationManager.AppSettings["Jwt:ExpirationMinutes"] ?? "60");
        }

        public JwtService(string secretKey, string encryptionKey, string issuer, string audience, int expirationMinutes)
        {
            _secretKey = secretKey;
            _encryptionKey = encryptionKey;
            _issuer = issuer;
            _audience = audience;
            _expirationMinutes = expirationMinutes;
        }

        /// <summary>
        /// Generate a JWE encrypted JWT token for a user
        /// </summary>
        public string GenerateToken(int userId, string email)
        {
            var signingKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_secretKey));
            var encryptingKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(PadKey(_encryptionKey, 32)));
            
            var signingCredentials = new SigningCredentials(signingKey, SecurityAlgorithms.HmacSha256);
            var encryptingCredentials = new EncryptingCredentials(
                encryptingKey, 
                SecurityAlgorithms.Aes256KW, 
                SecurityAlgorithms.Aes256CbcHmacSha512);

            var claims = new[]
            {
                new Claim(JwtRegisteredClaimNames.Sub, userId.ToString()),
                new Claim(JwtRegisteredClaimNames.Email, email),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                new Claim(JwtRegisteredClaimNames.Iat, DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString(), ClaimValueTypes.Integer64),
                new Claim("userId", userId.ToString())
            };

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                Expires = DateTime.UtcNow.AddMinutes(_expirationMinutes),
                Issuer = _issuer,
                Audience = _audience,
                SigningCredentials = signingCredentials,
                EncryptingCredentials = encryptingCredentials
            };

            var tokenHandler = new JwtSecurityTokenHandler();
            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }

        /// <summary>
        /// Validate a JWE encrypted JWT token and return the claims principal
        /// </summary>
        public ClaimsPrincipal ValidateToken(string token)
        {
            var signingKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_secretKey));
            var encryptingKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(PadKey(_encryptionKey, 32)));

            var tokenHandler = new JwtSecurityTokenHandler();
            var validationParameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateLifetime = true,
                ValidateIssuerSigningKey = true,
                ValidIssuer = _issuer,
                ValidAudience = _audience,
                IssuerSigningKey = signingKey,
                TokenDecryptionKey = encryptingKey,
                ClockSkew = TimeSpan.FromMinutes(5)
            };

            try
            {
                var principal = tokenHandler.ValidateToken(token, validationParameters, out _);
                return principal;
            }
            catch
            {
                return null;
            }
        }

        /// <summary>
        /// Get expiration time for the token
        /// </summary>
        public DateTime GetExpirationTime()
        {
            return DateTime.UtcNow.AddMinutes(_expirationMinutes);
        }

        /// <summary>
        /// Extract user ID from claims principal
        /// </summary>
        public static int? GetUserIdFromPrincipal(ClaimsPrincipal principal)
        {
            var userIdClaim = principal?.FindFirst("userId") ?? principal?.FindFirst(ClaimTypes.NameIdentifier);
            if (userIdClaim != null && int.TryParse(userIdClaim.Value, out int userId))
            {
                return userId;
            }
            return null;
        }

        private string PadKey(string key, int length)
        {
            if (key.Length >= length)
            {
                return key.Substring(0, length);
            }
            return key.PadRight(length, '0');
        }
    }
}
