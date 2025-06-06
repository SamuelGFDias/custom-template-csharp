using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;

namespace Infra.CrossCutting.Utils;

public static partial class Validate
{
    private static readonly EmailAddressAttribute EmailAttribute = new();

    [GeneratedRegex(@"^(?=.*[A-Z])(?=.*[a-z])(?=.*\d)(?=.*[^\w\s]).{8,20}$", RegexOptions.Compiled)]
    private static partial Regex PasswordRegex();
    
    [GeneratedRegex(@"^(?:\+?55\s*)?\(?(\d{2})\)?[\s-]*(\d{4,5})[\s-]*(\d{4})$", RegexOptions.Compiled)]
    private static partial Regex PhoneRegex();

    /// <summary>
    /// Tenta normalizar um telefone brasileiro em uma string contendo apenas DDD + número (10 ou 11 dígitos).
    /// Ex: "(21) 96941-5421" → "21969415421"
    /// </summary>
    /// <param name="input">texto de telefone a validar</param>
    /// <param name="normalized">
    /// saída, apenas dígitos do DDD + assinante, se válido; senão string.Empty
    /// </param>
    /// <returns>true se o formato for reconhecido e normalizado corretamente</returns>
    public static bool TryNormalizePhone(string? input, out string normalized)
    {
        normalized = string.Empty;
        if (string.IsNullOrWhiteSpace(input))
            return false;

        Match m = PhoneRegex().Match(input.Trim());
        if (!m.Success)
            return false;

        // grupos: 1 = DDD, 2 = primeira parte (4 ou 5 dígitos), 3 = última parte (4 dígitos)
        normalized = m.Groups[1].Value
                   + m.Groups[2].Value
                   + m.Groups[3].Value;
        return true;
    }

    /// <summary>
    /// Verifica apenas se o telefone é válido (padrão brasileiro com DDD).
    /// </summary>
    public static bool IsValidPhone(string? input, out string validPhone)
        => TryNormalizePhone(input, out validPhone);

    public static bool IsValidPassword(string? password)
        => password is { Length: >= 8 and <= 20 }
        && PasswordRegex().IsMatch(password);

    public static bool IsValidEmail(string? email)
    {
        return EmailAttribute.IsValid(email);
    }

    public static bool IsValidCpf(string? cpf)
    {
        string cpfClean = ExtractDigits(cpf);

        if (cpfClean.Length != 11 || cpfClean.Distinct().Count() == 1)
            return false;

        int[] multiplicador1 = [10, 9, 8, 7, 6, 5, 4, 3, 2];
        int[] multiplicador2 = [11, 10, 9, 8, 7, 6, 5, 4, 3, 2];

        string tempCpf = cpfClean[..9];
        int soma = tempCpf.Select((t, i) => (t - '0') * multiplicador1[i]).Sum();
        int resto = soma % 11;
        int digito1 = resto < 2 ? 0 : 11 - resto;

        tempCpf += digito1;
        soma = tempCpf.Select((t, i) => (t - '0') * multiplicador2[i]).Sum();
        resto = soma % 11;
        int digito2 = resto < 2 ? 0 : 11 - resto;

        return cpfClean.EndsWith($"{digito1}{digito2}");
    }

    public static bool IsValidCnpj(string? cnpj)
    {
        string cnpjClean = ExtractDigits(cnpj, 14);

        if (cnpjClean.Length != 14 || cnpjClean.Distinct().Count() == 1)
            return false;

        int[] multiplicador1 = [5, 4, 3, 2, 9, 8, 7, 6, 5, 4, 3, 2];
        int[] multiplicador2 = [6, 5, 4, 3, 2, 9, 8, 7, 6, 5, 4, 3, 2];

        string tempCnpj = cnpjClean[..12];
        int soma = tempCnpj.Select((t, i) => (t - '0') * multiplicador1[i]).Sum();
        int resto = soma % 11;
        int digito1 = resto < 2 ? 0 : 11 - resto;

        tempCnpj += digito1;
        soma = tempCnpj.Select((t, i) => (t - '0') * multiplicador2[i]).Sum();
        resto = soma % 11;
        int digito2 = resto < 2 ? 0 : 11 - resto;

        return cnpjClean.EndsWith($"{digito1}{digito2}");
    }

    private static string ExtractDigits(ReadOnlySpan<char> value, int length = 11)
    {
        Span<char> buffer = stackalloc char[length];
        int index = 0;

        foreach (char c in value)
        {
            if (!char.IsDigit(c)) continue;
            buffer[index++] = c;
            if (index == length) break;
        }

        return new string(buffer[..index]);
    }
}