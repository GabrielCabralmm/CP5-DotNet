using Estoque.Domain.Common;
using Estoque.Domain.Exceptions;

namespace Estoque.Domain.Entities;

public sealed class Produto : Entity
{
    private Produto()
    {
    }

    public Produto(string nome, string? descricao, string sku, decimal preco, int quantidadeEstoque, int fornecedorId)
    {
        Nome = Guard.Required(nome, "Nome", 120);
        Descricao = Guard.Optional(descricao, "Descricao", 500);
        Sku = Guard.Required(sku, "Sku", 40).ToUpperInvariant();
        Preco = ValidarPreco(preco);
        QuantidadeEstoque = quantidadeEstoque >= 0
            ? quantidadeEstoque
            : throw new DomainValidationException("Produto.QuantidadeInvalida", "A quantidade inicial em estoque não pode ser negativa.");
        FornecedorId = ValidarFornecedor(fornecedorId);
    }

    public string Nome { get; private set; } = string.Empty;

    public string? Descricao { get; private set; }

    public string Sku { get; private set; } = string.Empty;

    public decimal Preco { get; private set; }

    public int QuantidadeEstoque { get; private set; }

    public int FornecedorId { get; private set; }

    public Fornecedor? Fornecedor { get; private set; }

    public void Atualizar(string nome, string? descricao, decimal preco, int fornecedorId)
    {
        Nome = Guard.Required(nome, "Nome", 120);
        Descricao = Guard.Optional(descricao, "Descricao", 500);
        Preco = ValidarPreco(preco);
        FornecedorId = ValidarFornecedor(fornecedorId);
        MarcarAtualizacao();
    }

    public void AjustarEstoque(int quantidade)
    {
        if (quantidade == 0)
            throw new DomainValidationException("Produto.AjusteInvalido", "A quantidade do ajuste deve ser diferente de zero.");

        var novoSaldo = QuantidadeEstoque + quantidade;
        if (novoSaldo < 0)
            throw new BusinessRuleException(
                "Produto.EstoqueInsuficiente",
                $"Estoque insuficiente para o produto {Sku}. Saldo atual: {QuantidadeEstoque}, saída solicitada: {Math.Abs(quantidade)}.");

        QuantidadeEstoque = novoSaldo;
        MarcarAtualizacao();
    }

    private static decimal ValidarPreco(decimal preco) =>
        preco > 0
            ? decimal.Round(preco, 2)
            : throw new DomainValidationException("Produto.PrecoInvalido", "O preço do produto deve ser maior que zero.");

    private static int ValidarFornecedor(int fornecedorId) =>
        fornecedorId > 0
            ? fornecedorId
            : throw new DomainValidationException("Produto.FornecedorObrigatorio", "O fornecedor do produto é obrigatório.");
}
