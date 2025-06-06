using System;

namespace Application.Contracts.Services.Auth;

public interface IJwtService
{
    Task<string> GenerateToken(string deviceId);
}
