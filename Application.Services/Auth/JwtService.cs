
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Application.Contracts.Services.Auth;
using Infra.CrossCutting.Providers;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace Application.Services.Auth;

public class JwtService(
    IOptions<JwtSecrets> jwtSecrets
) : IJwtService
{
    private readonly JwtSecrets _jwtSecrets = jwtSecrets.Value;

    public Task<string> GenerateToken(string deviceId)
    {
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtSecrets.SecretKey));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Jti, Guid.CreateVersion7().ToString()),
            new Claim(JwtRegisteredClaimNames.Typ, deviceId),
        };

        var tokenOptions = new JwtSecurityToken(
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(30),
            signingCredentials: credentials
        );

        string? token = new JwtSecurityTokenHandler().WriteToken(tokenOptions);
        return Task.FromResult(token);
    }
}