using Estoque.Application.DTOs;
using Estoque.Domain.Common;
using Estoque.Domain.Entities;

namespace Estoque.Application.Interfaces;

public interface IFornecedorService
{
    Task<OperationResult<PageResult<FornecedorResponse>>> GetPagedAsync(int pageNumber, int pageSize, string? uf, CancellationToken ct = default);

    Task<OperationResult<FornecedorResponse>> GetByIdAsync(int id, CancellationToken ct = default);

    Task<OperationResult<FornecedorResponse>> CreateAsync(CreateFornecedorRequest request, CancellationToken ct = default);

    Task<OperationResult<FornecedorResponse>> UpdateAsync(int id, UpdateFornecedorRequest request, CancellationToken ct = default);

    Task<OperationResult> DeleteAsync(int id, CancellationToken ct = default);
}

public interface IProdutoService
{
    Task<OperationResult<PageResult<ProdutoResponse>>> GetPagedAsync(int pageNumber, int pageSize, int? fornecedorId, string? nome, CancellationToken ct = default);

    Task<OperationResult<ProdutoResponse>> GetByIdAsync(int id, CancellationToken ct = default);

    Task<OperationResult<ProdutoResponse>> CreateAsync(CreateProdutoRequest request, CancellationToken ct = default);

    Task<OperationResult<ProdutoResponse>> UpdateAsync(int id, UpdateProdutoRequest request, CancellationToken ct = default);

    Task<OperationResult<ProdutoResponse>> AjustarEstoqueAsync(int id, int quantidade, CancellationToken ct = default);

    Task<OperationResult> DeleteAsync(int id, CancellationToken ct = default);
}

public interface IAuthService
{
    Task<OperationResult<TokenResponse>> LoginAsync(LoginRequest request, CancellationToken ct = default);
}

public interface ICepService
{
    Task<OperationResult<EnderecoCepResponse>> ConsultarAsync(string cep, CancellationToken ct = default);
}

public interface ITokenService
{
    TokenResponse GerarToken(Usuario usuario);
}

public interface IPasswordHasher
{
    string Hash(string password);

    bool Verify(string password, string passwordHash);
}
