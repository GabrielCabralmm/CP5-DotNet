using System.ComponentModel.DataAnnotations;

namespace Estoque.Application.DTOs;

public sealed record CreateProdutoRequest(
    [Required, StringLength(120, MinimumLength = 2)] string Nome,
    [StringLength(500)] string? Descricao,
    [Required, StringLength(40, MinimumLength = 3)] string Sku,
    [Range(0.01, 9999999.0)] decimal Preco,
    [Range(0, 1000000)] int QuantidadeEstoque,
    [Range(1, int.MaxValue)] int FornecedorId);

public sealed record UpdateProdutoRequest(
    [Required, StringLength(120, MinimumLength = 2)] string Nome,
    [StringLength(500)] string? Descricao,
    [Range(0.01, 9999999.0)] decimal Preco,
    [Range(1, int.MaxValue)] int FornecedorId);

public sealed record AjusteEstoqueRequest(
    [Range(-1000000, 1000000)] int Quantidade);

public sealed record ProdutoResponse(
    int Id,
    string Nome,
    string? Descricao,
    string Sku,
    decimal Preco,
    int QuantidadeEstoque,
    int FornecedorId,
    string? FornecedorNome,
    DateTime CriadoEm,
    DateTime? AtualizadoEm);
