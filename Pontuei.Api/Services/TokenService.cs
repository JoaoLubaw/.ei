using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using Pontuei.Api.Interfaces.Services;
using Pontuei.Api.Models;

namespace Pontuei.Api.Services;

public class TokenService : ITokenService
{
    private readonly IConfiguration _configuration;
    private readonly IConfigurationService _configurationService;

    public TokenService(
        IConfiguration configuration,
        IConfigurationService configurationService)
    {
        _configuration = configuration;
        _configurationService = configurationService;
    }

    public async Task<string> GenerateAccessToken(User user)
    {
        JwtSecurityTokenHandler tokenHandler = new JwtSecurityTokenHandler();
        byte[] key = Encoding.ASCII.GetBytes(_configuration["JWT:Secret"]!);
        int jwtExpirationMinutes = await _configurationService.GetJWTExpirationMinutes();

        List<Claim> claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, user.UserId.ToString()),
            new Claim("name", user.UserName),
            new Claim("email", user.UserEmail),
        };

        if (user.UserIsAdmin)
        {
            claims.Add(new Claim(ClaimTypes.Role, "Admin"));
        }

        SecurityTokenDescriptor tokenDescriptor = new SecurityTokenDescriptor
        {
            Issuer = _configuration["JWT:Issuer"],
            Audience = _configuration["JWT:Audience"],
            Subject = new ClaimsIdentity(claims),
            NotBefore = DateTime.UtcNow,
            Expires = DateTime.UtcNow.AddMinutes(jwtExpirationMinutes),
            IssuedAt = DateTime.UtcNow,
            TokenType = "at+jwt",
            SigningCredentials = new SigningCredentials(
                new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature
            )
        };

        SecurityToken token = tokenHandler.CreateToken(tokenDescriptor);
        return tokenHandler.WriteToken(token);
    }

    public string GenerateRefreshToken()
    {
        byte[] randomBytes = new byte[64];
        using (var rng = RandomNumberGenerator.Create())
        {
            rng.GetBytes(randomBytes);
        }
        return Convert.ToBase64String(randomBytes);
    }
}