using Estoque.Domain.Common;
using Estoque.Domain.Entities;
using Estoque.Domain.Errors;
using Estoque.Domain.Exceptions;
using Estoque.Domain.Interfaces;
using Estoque.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Estoque.Infrastructure.Repositories;

public sealed class FornecedorRepository(EstoqueDbContext context) : IFornecedorRepository
{
    public async Task<PageResult<Fornecedor>> GetPagedAsync(PageRequest page, string? uf, CancellationToken ct = default)
    {
        var query = context.Fornecedores.AsNoTracking();

        if (!string.IsNullOrWhiteSpace(uf))
        {
            var ufFiltro = uf.Trim().ToUpperInvariant();
            query = query.Where(f => f.Uf == ufFiltro);
        }

        var total = await query.CountAsync(ct);
        var itens = await query
            .OrderBy(f => f.Id)
            .Skip(page.Skip)
            .Take(page.PageSize)
            .ToListAsync(ct);

        return new PageResult<Fornecedor>(itens, page.PageNumber, page.PageSize, total);
    }

    public async Task<OperationResult<Fornecedor>> GetByIdAsync(int id, CancellationToken ct = default)
    {
        var fornecedor = await context.Fornecedores
            .AsNoTracking()
            .FirstOrDefaultAsync(f => f.Id == id, ct);

        return fornecedor is null
            ? FornecedorErrors.NaoEncontrado(id)
            : OperationResult<Fornecedor>.Success(fornecedor);
    }

    public Task<bool> ExistsAsync(int id, CancellationToken ct = default) =>
        context.Fornecedores.AsNoTracking().AnyAsync(f => f.Id == id, ct);

    public async Task<Fornecedor> AddAsync(Fornecedor fornecedor, CancellationToken ct = default)
    {
        var cnpj = fornecedor.Cnpj;
        if (await context.Fornecedores.AsNoTracking().AnyAsync(f => f.Cnpj == cnpj, ct))
            throw new DuplicateEntityException(FornecedorErrors.CnpjDuplicado(cnpj));

        context.Fornecedores.Add(fornecedor);

        try
        {
            await context.SaveChangesAsync(ct);
        }
        catch (DbUpdateException ex)
        {
            throw new RepositoryException("Falha ao inserir o fornecedor no banco de dados.", ex);
        }

        return fornecedor;
    }

    public async Task<OperationResult> UpdateAsync(Fornecedor fornecedor, CancellationToken ct = default)
    {
        context.Fornecedores.Update(fornecedor);
        return await SalvarAsync(ct);
    }

    public async Task<OperationResult> DeleteAsync(int id, CancellationToken ct = default)
    {
        var fornecedor = await context.Fornecedores.FirstOrDefaultAsync(f => f.Id == id, ct);
        if (fornecedor is null)
            return FornecedorErrors.NaoEncontrado(id);

        if (await context.Produtos.AsNoTracking().AnyAsync(p => p.FornecedorId == id, ct))
            return FornecedorErrors.PossuiProdutos(id);

        context.Fornecedores.Remove(fornecedor);
        return await SalvarAsync(ct);
    }

    private async Task<OperationResult> SalvarAsync(CancellationToken ct)
    {
        try
        {
            await context.SaveChangesAsync(ct);
            return OperationResult.Success();
        }
        catch (DbUpdateException ex)
        {
            return FornecedorErrors.FalhaAoSalvar(ex.GetBaseException().Message);
        }
    }
}
