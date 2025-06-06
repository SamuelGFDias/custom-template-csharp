using Infra.CrossCutting.Attributes;

namespace Infra.CrossCutting.Providers;

[ProviderConfig("Jwt")]
public class JwtSecrets : IProvider
{
    public required string KeyId { get; set; }
    public required string SecretKey { get; set; }
}