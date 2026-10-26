using Estoque.Domain.Common;
using Estoque.Domain.Entities;

namespace Estoque.Domain.Interfaces;

public interface IFornecedorRepository
{
    Task<PageResult<Fornecedor>> GetPagedAsync(PageRequest page, string? uf, CancellationToken ct = default);

    Task<OperationResult<Fornecedor>> GetByIdAsync(int id, CancellationToken ct = default);

    Task<bool> ExistsAsync(int id, CancellationToken ct = default);

    Task<Fornecedor> AddAsync(Fornecedor fornecedor, CancellationToken ct = default);

    Task<OperationResult> UpdateAsync(Fornecedor fornecedor, CancellationToken ct = default);

    Task<OperationResult> DeleteAsync(int id, CancellationToken ct = default);
}

public interface IProdutoRepository
{
    Task<PageResult<Produto>> GetPagedAsync(PageRequest page, int? fornecedorId, string? nome, CancellationToken ct = default);

    Task<Produto> GetByIdAsync(int id, CancellationToken ct = default);

    Task<Produto> GetForUpdateAsync(int id, CancellationToken ct = default);

    Task<Produto> AddAsync(Produto produto, CancellationToken ct = default);

    Task UpdateAsync(Produto produto, CancellationToken ct = default);

    Task DeleteAsync(int id, CancellationToken ct = default);
}

public interface IUsuarioRepository
{
    Task<Usuario?> GetByUsernameAsync(string username, CancellationToken ct = default);
}
