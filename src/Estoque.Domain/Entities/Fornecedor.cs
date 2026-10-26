using Estoque.Domain.Common;
using Estoque.Domain.ValueObjects;

namespace Estoque.Domain.Entities;

public sealed class Fornecedor : Entity
{
    private Fornecedor()
    {
    }

    public Fornecedor(string razaoSocial, string cnpj, string email, string? telefone, Endereco endereco)
    {
        RazaoSocial = Guard.Required(razaoSocial, "RazaoSocial", 150);
        Cnpj = Guard.Digits(cnpj, "Cnpj", 14);
        Email = Guard.Email(email, "Email");
        Telefone = Guard.Optional(telefone, "Telefone", 20);
        AplicarEndereco(endereco);
    }

    public string RazaoSocial { get; private set; } = string.Empty;

    public string Cnpj { get; private set; } = string.Empty;

    public string Email { get; private set; } = string.Empty;

    public string? Telefone { get; private set; }

    public string Cep { get; private set; } = string.Empty;

    public string? Logradouro { get; private set; }

    public string? Bairro { get; private set; }

    public string Cidade { get; private set; } = string.Empty;

    public string Uf { get; private set; } = string.Empty;

    public ICollection<Produto> Produtos { get; private set; } = new List<Produto>();

    public void Atualizar(string razaoSocial, string email, string? telefone, Endereco? novoEndereco)
    {
        RazaoSocial = Guard.Required(razaoSocial, "RazaoSocial", 150);
        Email = Guard.Email(email, "Email");
        Telefone = Guard.Optional(telefone, "Telefone", 20);

        if (novoEndereco is not null)
            AplicarEndereco(novoEndereco);

        MarcarAtualizacao();
    }

    private void AplicarEndereco(Endereco endereco)
    {
        Cep = Guard.Digits(endereco.Cep, "Cep", 8);
        Logradouro = Guard.Optional(endereco.Logradouro, "Logradouro", 150);
        Bairro = Guard.Optional(endereco.Bairro, "Bairro", 100);
        Cidade = Guard.Required(endereco.Cidade, "Cidade", 100);
        Uf = Guard.Required(endereco.Uf, "Uf", 2).ToUpperInvariant();
    }
}
