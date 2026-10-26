using System.ComponentModel.DataAnnotations;

namespace Estoque.Application.DTOs;

public sealed record CreateFornecedorRequest(
    [Required, StringLength(150, MinimumLength = 3)] string RazaoSocial,
    [Required, StringLength(18, MinimumLength = 14)] string Cnpj,
    [Required, EmailAddress, StringLength(150)] string Email,
    [StringLength(20)] string? Telefone,
    [Required, StringLength(9, MinimumLength = 8)] string Cep);

public sealed record UpdateFornecedorRequest(
    [Required, StringLength(150, MinimumLength = 3)] string RazaoSocial,
    [Required, EmailAddress, StringLength(150)] string Email,
    [StringLength(20)] string? Telefone,
    [Required, StringLength(9, MinimumLength = 8)] string Cep);

public sealed record FornecedorResponse(
    int Id,
    string RazaoSocial,
    string Cnpj,
    string Email,
    string? Telefone,
    string Cep,
    string? Logradouro,
    string? Bairro,
    string Cidade,
    string Uf,
    DateTime CriadoEm,
    DateTime? AtualizadoEm);
