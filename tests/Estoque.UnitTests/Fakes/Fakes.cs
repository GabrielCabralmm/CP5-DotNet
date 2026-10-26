using System.Net;
using Estoque.Application.DTOs;
using Estoque.Application.Interfaces;
using Estoque.Domain.Common;
using Estoque.Domain.Entities;
using Estoque.Domain.Errors;
using Estoque.Domain.Exceptions;
using Estoque.Domain.Interfaces;
using Estoque.Domain.ValueObjects;

namespace Estoque.UnitTests.Fakes;

internal static class TestData
{
    public static T ComId<T>(T entity, int id) where T : Entity
    {
        typeof(Entity).GetProperty(nameof(Entity.Id))!.SetValue(entity, id);
        return entity;
    }

    public static Endereco EnderecoPadrao() => new("01310100", "Avenida Paulista", "Bela Vista", "São Paulo", "SP");

    public static Fornecedor Fornecedor(int id = 1) =>
        ComId(new Fornecedor("Fornecedor Teste LTDA", "11222333000181", "contato@teste.com.br", null, EnderecoPadrao()), id);

    public static Produto Produto(int id = 1, int estoque = 10, int fornecedorId = 1) =>
        ComId(new Produto("Mouse", "Mouse sem fio", "SKU-001", 99.90m, estoque, fornecedorId), id);
}

internal sealed class FakeFornecedorRepository : IFornecedorRepository
{
    public List<Fornecedor> Itens { get; } = new();

    public bool LancarDuplicado { get; set; }

    public Task<PageResult<Fornecedor>> GetPagedAsync(PageRequest page, string? uf, CancellationToken ct = default)
    {
        var filtrados = Itens.Where(f => uf is null || f.Uf == uf).ToList();
        var itens = filtrados.Skip(page.Skip).Take(page.PageSize).ToList();
        return Task.FromResult(new PageResult<Fornecedor>(itens, page.PageNumber, page.PageSize, filtrados.Count));
    }

    public Task<OperationResult<Fornecedor>> GetByIdAsync(int id, CancellationToken ct = default)
    {
        var fornecedor = Itens.FirstOrDefault(f => f.Id == id);
        OperationResult<Fornecedor> result = fornecedor is null
            ? FornecedorErrors.NaoEncontrado(id)
            : OperationResult<Fornecedor>.Success(fornecedor);
        return Task.FromResult(result);
    }

    public Task<bool> ExistsAsync(int id, CancellationToken ct = default) =>
        Task.FromResult(Itens.Any(f => f.Id == id));

    public Task<Fornecedor> AddAsync(Fornecedor fornecedor, CancellationToken ct = default)
    {
        if (LancarDuplicado)
            throw new DuplicateEntityException(FornecedorErrors.CnpjDuplicado(fornecedor.Cnpj));

        TestData.ComId(fornecedor, Itens.Count + 1);
        Itens.Add(fornecedor);
        return Task.FromResult(fornecedor);
    }

    public Task<OperationResult> UpdateAsync(Fornecedor fornecedor, CancellationToken ct = default) =>
        Task.FromResult(OperationResult.Success());

    public Task<OperationResult> DeleteAsync(int id, CancellationToken ct = default)
    {
        var fornecedor = Itens.FirstOrDefault(f => f.Id == id);
        if (fornecedor is null)
            return Task.FromResult<OperationResult>(FornecedorErrors.NaoEncontrado(id));

        Itens.Remove(fornecedor);
        return Task.FromResult(OperationResult.Success());
    }
}

internal sealed class FakeProdutoRepository : IProdutoRepository
{
    public List<Produto> Itens { get; } = new();

    public Task<PageResult<Produto>> GetPagedAsync(PageRequest page, int? fornecedorId, string? nome, CancellationToken ct = default)
    {
        var itens = Itens.Skip(page.Skip).Take(page.PageSize).ToList();
        return Task.FromResult(new PageResult<Produto>(itens, page.PageNumber, page.PageSize, Itens.Count));
    }

    public Task<Produto> GetByIdAsync(int id, CancellationToken ct = default) =>
        Task.FromResult(Itens.FirstOrDefault(p => p.Id == id) ?? throw new EntityNotFoundException(ProdutoErrors.NaoEncontrado(id)));

    public Task<Produto> GetForUpdateAsync(int id, CancellationToken ct = default) => GetByIdAsync(id, ct);

    public Task<Produto> AddAsync(Produto produto, CancellationToken ct = default)
    {
        if (Itens.Any(p => p.Sku == produto.Sku))
            throw new DuplicateEntityException(ProdutoErrors.SkuDuplicado(produto.Sku));

        TestData.ComId(produto, Itens.Count + 1);
        Itens.Add(produto);
        return Task.FromResult(produto);
    }

    public Task UpdateAsync(Produto produto, CancellationToken ct = default) => Task.CompletedTask;

    public Task DeleteAsync(int id, CancellationToken ct = default)
    {
        var produto = Itens.FirstOrDefault(p => p.Id == id) ?? throw new EntityNotFoundException(ProdutoErrors.NaoEncontrado(id));
        Itens.Remove(produto);
        return Task.CompletedTask;
    }
}

internal sealed class FakeCepService : ICepService
{
    public Error? ErroForcado { get; set; }

    public int Chamadas { get; private set; }

    public Task<OperationResult<EnderecoCepResponse>> ConsultarAsync(string cep, CancellationToken ct = default)
    {
        Chamadas++;
        OperationResult<EnderecoCepResponse> result = ErroForcado is not null
            ? ErroForcado
            : OperationResult<EnderecoCepResponse>.Success(new EnderecoCepResponse(Guard.OnlyDigits(cep), "Praça da Sé", "Sé", "São Paulo", "SP"));
        return Task.FromResult(result);
    }
}

internal sealed class FakeUsuarioRepository(params Usuario[] usuarios) : IUsuarioRepository
{
    public Task<Usuario?> GetByUsernameAsync(string username, CancellationToken ct = default) =>
        Task.FromResult(usuarios.FirstOrDefault(u => u.Username == username));
}

internal sealed class FakeTokenService : ITokenService
{
    public TokenResponse GerarToken(Usuario usuario) =>
        new("token-fake", "Bearer", DateTime.UtcNow.AddHours(1), usuario.Username, usuario.Perfil);
}

internal sealed class StubHttpMessageHandler(HttpStatusCode statusCode, string conteudo) : HttpMessageHandler
{
    public int Chamadas { get; private set; }

    protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        Chamadas++;
        return Task.FromResult(new HttpResponseMessage(statusCode)
        {
            Content = new StringContent(conteudo, System.Text.Encoding.UTF8, "application/json")
        });
    }
}

internal sealed class FailingHttpMessageHandler : HttpMessageHandler
{
    protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken) =>
        throw new HttpRequestException("Falha de rede simulada");
}
