using Pontuei.Api.Models;

namespace Pontuei.Api.Interfaces.Services;

public interface ITokenService
{
    Task<string> GenerateAccessToken(User user);
    string GenerateRefreshToken();
}