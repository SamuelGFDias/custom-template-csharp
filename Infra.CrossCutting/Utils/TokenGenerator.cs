using System.Security.Cryptography;

namespace Infra.CrossCutting.Utils;

public static class TokenGenerator
{
    public static string GenerateSecureToken(int byteLength = 32)
    {
        // Gera bytes aleatórios criptograficamente seguros
        byte[] randomBytes = RandomNumberGenerator.GetBytes(byteLength);

        // Converte para uma string Base64 (mais compacta que hexadecimal)
        string token = Convert.ToBase64String(randomBytes)
                              .Replace("+", "-") // Substitui caracteres não URL-safe
                              .Replace("/", "_")
                              .Replace("=", ""); // Remove padding

        return token;
    }
}