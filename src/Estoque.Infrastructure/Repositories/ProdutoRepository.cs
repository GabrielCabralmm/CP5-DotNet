using Estoque.Domain.Common;
using Estoque.Domain.Entities;
using Estoque.Domain.Errors;
using Estoque.Domain.Exceptions;
using Estoque.Domain.Interfaces;
using Estoque.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Estoque.Infrastructure.Repositories;

public sealed class ProdutoRepository(EstoqueDbContext context) : IProdutoRepository
{
    public async Task<PageResult<Produto>> GetPagedAsync(PageRequest page, int? fornecedorId, string? nome, CancellationToken ct = default)
    {
        var query = context.Produtos.AsNoTracking();

        if (fornecedorId.HasValue)
        {
            var idFornecedor = fornecedorId.Value;
            query = query.Where(p => p.FornecedorId == idFornecedor);
        }

        if (!string.IsNullOrWhiteSpace(nome))
        {
            var termo = nome.Trim().ToUpper();
            query = query.Where(p => p.Nome.ToUpper().Contains(termo));
        }

        var total = await query.CountAsync(ct);
        var itens = await query
            .Include(p => p.Fornecedor)
            .OrderBy(p => p.Id)
            .Skip(page.Skip)
            .Take(page.PageSize)
            .ToListAsync(ct);

        return new PageResult<Produto>(itens, page.PageNumber, page.PageSize, total);
    }

    public async Task<Produto> GetByIdAsync(int id, CancellationToken ct = default) =>
        await context.Produtos
            .AsNoTracking()
            .Include(p => p.Fornecedor)
            .FirstOrDefaultAsync(p => p.Id == id, ct)
        ?? throw new EntityNotFoundException(ProdutoErrors.NaoEncontrado(id));

    public async Task<Produto> GetForUpdateAsync(int id, CancellationToken ct = default) =>
        await context.Produtos.FirstOrDefaultAsync(p => p.Id == id, ct)
        ?? throw new EntityNotFoundException(ProdutoErrors.NaoEncontrado(id));

    public async Task<Produto> AddAsync(Produto produto, CancellationToken ct = default)
    {
        var sku = produto.Sku;
        if (await context.Produtos.AsNoTracking().AnyAsync(p => p.Sku == sku, ct))
            throw new DuplicateEntityException(ProdutoErrors.SkuDuplicado(sku));

        context.Produtos.Add(produto);
        await SalvarAsync("inserir", ct);
        return produto;
    }

    public async Task UpdateAsync(Produto produto, CancellationToken ct = default)
    {
        context.Produtos.Update(produto);
        await SalvarAsync("atualizar", ct);
    }

    public async Task DeleteAsync(int id, CancellationToken ct = default)
    {
        var removidos = await context.Produtos
            .Where(p => p.Id == id)
            .ExecuteDeleteAsync(ct);

        if (removidos == 0)
            throw new EntityNotFoundException(ProdutoErrors.NaoEncontrado(id));
    }

    private async Task SalvarAsync(string operacao, CancellationToken ct)
    {
        try
        {
            await context.SaveChangesAsync(ct);
        }
        catch (DbUpdateException ex)
        {
            throw new RepositoryException($"Falha ao {operacao} o produto no banco de dados.", ex);
        }
    }
}
