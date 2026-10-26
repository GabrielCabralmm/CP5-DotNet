using Estoque.Domain.Entities;
using Estoque.Domain.Exceptions;
using Estoque.UnitTests.Fakes;

namespace Estoque.UnitTests.Domain;

public sealed class FornecedorTests
{
    [Fact]
    public void Criar_ComDadosValidos_NormalizaCnpjCepEUf()
    {
        var fornecedor = new Fornecedor("Fornecedor X", "11.222.333/0001-81", "CONTATO@X.COM.BR", null,
            new("01310-100", "Av. Paulista", "Bela Vista", "São Paulo", "sp"));

        Assert.Equal("11222333000181", fornecedor.Cnpj);
        Assert.Equal("01310100", fornecedor.Cep);
        Assert.Equal("SP", fornecedor.Uf);
        Assert.Equal("contato@x.com.br", fornecedor.Email);
    }

    [Theory]
    [InlineData("123")]
    [InlineData("")]
    [InlineData("1122233300018199")]
    public void Criar_ComCnpjInvalido_LancaDomainValidationException(string cnpj)
    {
        var ex = Assert.Throws<DomainValidationException>(() =>
            new Fornecedor("Fornecedor X", cnpj, "contato@x.com.br", null, TestData.EnderecoPadrao()));

        Assert.Equal("Cnpj.FormatoInvalido", ex.Code);
    }

    [Fact]
    public void Criar_ComEmailInvalido_LancaDomainValidationException()
    {
        var ex = Assert.Throws<DomainValidationException>(() =>
            new Fornecedor("Fornecedor X", "11222333000181", "email-invalido", null, TestData.EnderecoPadrao()));

        Assert.Equal("Email.FormatoInvalido", ex.Code);
    }

    [Fact]
    public void Atualizar_SemNovoEndereco_MantemEnderecoEAtualizaData()
    {
        var fornecedor = TestData.Fornecedor();

        fornecedor.Atualizar("Novo Nome", "novo@x.com.br", "(11) 99999-0000", null);

        Assert.Equal("Novo Nome", fornecedor.RazaoSocial);
        Assert.Equal("01310100", fornecedor.Cep);
        Assert.NotNull(fornecedor.AtualizadoEm);
    }
}

public sealed class ProdutoTests
{
    [Fact]
    public void AjustarEstoque_Entrada_SomaAoSaldo()
    {
        var produto = TestData.Produto(estoque: 10);

        produto.AjustarEstoque(5);

        Assert.Equal(15, produto.QuantidadeEstoque);
    }

    [Fact]
    public void AjustarEstoque_SaidaMaiorQueSaldo_LancaBusinessRuleException()
    {
        var produto = TestData.Produto(estoque: 3);

        var ex = Assert.Throws<BusinessRuleException>(() => produto.AjustarEstoque(-5));

        Assert.Equal("Produto.EstoqueInsuficiente", ex.Code);
        Assert.Equal(3, produto.QuantidadeEstoque);
    }

    [Fact]
    public void AjustarEstoque_Zero_LancaDomainValidationException()
    {
        var produto = TestData.Produto();

        Assert.Throws<DomainValidationException>(() => produto.AjustarEstoque(0));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-10)]
    public void Criar_ComPrecoInvalido_LancaDomainValidationException(int preco)
    {
        var ex = Assert.Throws<DomainValidationException>(() => new Produto("Mouse", null, "SKU-1", preco, 1, 1));

        Assert.Equal("Produto.PrecoInvalido", ex.Code);
    }

    [Fact]
    public void Criar_NormalizaSkuParaMaiusculo()
    {
        var produto = new Produto("Mouse", null, "sku-abc", 10m, 1, 1);

        Assert.Equal("SKU-ABC", produto.Sku);
    }
}
