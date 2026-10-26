namespace Estoque.Domain.ValueObjects;

public sealed record Endereco(string Cep, string? Logradouro, string? Bairro, string Cidade, string Uf);
