using Estoque.Application.DTOs;
using Estoque.Domain.Entities;
using Estoque.Domain.ValueObjects;

namespace Estoque.Application.Mappings;

public static class MappingExtensions
{
    public static FornecedorResponse ToResponse(this Fornecedor f) =>
        new(f.Id, f.RazaoSocial, f.Cnpj, f.Email, f.Telefone, f.Cep, f.Logradouro, f.Bairro, f.Cidade, f.Uf, f.CriadoEm, f.AtualizadoEm);

    public static ProdutoResponse ToResponse(this Produto p, string? fornecedorNome = null) =>
        new(p.Id, p.Nome, p.Descricao, p.Sku, p.Preco, p.QuantidadeEstoque, p.FornecedorId,
            p.Fornecedor?.RazaoSocial ?? fornecedorNome, p.CriadoEm, p.AtualizadoEm);

    public static Endereco ToEndereco(this EnderecoCepResponse cep) =>
        new(cep.Cep, cep.Logradouro, cep.Bairro, cep.Cidade, cep.Uf);
}
