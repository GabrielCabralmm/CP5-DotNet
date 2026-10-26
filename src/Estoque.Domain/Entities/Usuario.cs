using Estoque.Domain.Common;
using Estoque.Domain.Exceptions;

namespace Estoque.Domain.Entities;

public sealed class Usuario : Entity
{
    private Usuario()
    {
    }

    public Usuario(string username, string passwordHash, string perfil)
    {
        Username = Guard.Required(username, "Username", 50).ToLowerInvariant();
        PasswordHash = Guard.Required(passwordHash, "PasswordHash", 200);
        Perfil = Perfis.Todos.Contains(perfil)
            ? perfil
            : throw new DomainValidationException("Usuario.PerfilInvalido", $"Perfil '{perfil}' inválido.");
    }

    public string Username { get; private set; } = string.Empty;

    public string PasswordHash { get; private set; } = string.Empty;

    public string Perfil { get; private set; } = string.Empty;
}

public static class Perfis
{
    public const string Admin = "Admin";
    public const string Leitor = "Leitor";

    public static readonly IReadOnlyCollection<string> Todos = new[] { Admin, Leitor };
}
