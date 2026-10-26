using System.Net.Mail;
using Estoque.Domain.Exceptions;

namespace Estoque.Domain.Common;

public static class Guard
{
    public static string Required(string? value, string field, int maxLength)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new DomainValidationException($"{field}.Obrigatorio", $"O campo {field} é obrigatório.");

        var trimmed = value.Trim();
        if (trimmed.Length > maxLength)
            throw new DomainValidationException($"{field}.TamanhoInvalido", $"O campo {field} deve ter no máximo {maxLength} caracteres.");

        return trimmed;
    }

    public static string? Optional(string? value, string field, int maxLength)
    {
        if (string.IsNullOrWhiteSpace(value))
            return null;

        var trimmed = value.Trim();
        if (trimmed.Length > maxLength)
            throw new DomainValidationException($"{field}.TamanhoInvalido", $"O campo {field} deve ter no máximo {maxLength} caracteres.");

        return trimmed;
    }

    public static string Digits(string? value, string field, int length)
    {
        var digits = OnlyDigits(value);
        if (digits.Length != length)
            throw new DomainValidationException($"{field}.FormatoInvalido", $"O campo {field} deve conter {length} dígitos.");

        return digits;
    }

    public static string Email(string? value, string field)
    {
        var email = Required(value, field, 150);
        if (!MailAddress.TryCreate(email, out _) || !email.Contains('.'))
            throw new DomainValidationException($"{field}.FormatoInvalido", $"O campo {field} não é um e-mail válido.");

        return email.ToLowerInvariant();
    }

    public static string OnlyDigits(string? value) =>
        new((value ?? string.Empty).Where(char.IsDigit).ToArray());
}
