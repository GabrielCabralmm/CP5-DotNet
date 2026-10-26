using System.ComponentModel.DataAnnotations;

namespace Estoque.Application.DTOs;

public sealed record LoginRequest(
    [Required] string Username,
    [Required] string Password);

public sealed record TokenResponse(string AccessToken, string TokenType, DateTime ExpiresAt, string Username, string Perfil);

public sealed record EnderecoCepResponse(string Cep, string? Logradouro, string? Bairro, string Cidade, string Uf);
