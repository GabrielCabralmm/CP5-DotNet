using Estoque.Application.DTOs;
using Estoque.Application.Interfaces;
using Estoque.Application.Mappings;
using Estoque.Domain.Common;
using Estoque.Domain.Entities;
using Estoque.Domain.Errors;
using Estoque.Domain.Exceptions;
using Estoque.Domain.Interfaces;

namespace Estoque.Application.Services;

public sealed class ProdutoService(IProdutoRepository repository, IFornecedorRepository fornecedorRepository) : IProdutoService
{
    public async Task<OperationResult<PageResult<ProdutoResponse>>> GetPagedAsync(int pageNumber, int pageSize, int? fornecedorId, string? nome, CancellationToken ct = default)
    {
        var page = await repository.GetPagedAsync(PageRequest.Create(pageNumber, pageSize), fornecedorId, nome, ct);
        return OperationResult<PageResult<ProdutoResponse>>.Success(page.Map(p => p.ToResponse()));
    }

    public async Task<OperationResult<ProdutoResponse>> GetByIdAsync(int id, CancellationToken ct = default)
    {
        try
        {
            var produto = await repository.GetByIdAsync(id, ct);
            return OperationResult<ProdutoResponse>.Success(produto.ToResponse());
        }
        catch (DomainException ex)
        {
            return ex.Error;
        }
    }

    public async Task<OperationResult<ProdutoResponse>> CreateAsync(CreateProdutoRequest request, CancellationToken ct = default)
    {
        try
        {
            var fornecedor = await fornecedorRepository.GetByIdAsync(request.FornecedorId, ct);
            if (fornecedor.IsFailure)
                return ProdutoErrors.FornecedorInexistente(request.FornecedorId);

            var produto = new Produto(
                request.Nome,
                request.Descricao,
                request.Sku,
                request.Preco,
                request.QuantidadeEstoque,
                request.FornecedorId);

            var criado = await repository.AddAsync(produto, ct);
            return OperationResult<ProdutoResponse>.Success(criado.ToResponse(fornecedor.Data!.RazaoSocial));
        }
        catch (DomainException ex)
        {
            return ex.Error;
        }
    }

    public async Task<OperationResult<ProdutoResponse>> UpdateAsync(int id, UpdateProdutoRequest request, CancellationToken ct = default)
    {
        try
        {
            var produto = await repository.GetForUpdateAsync(id, ct);

            if (request.FornecedorId != produto.FornecedorId && !await fornecedorRepository.ExistsAsync(request.FornecedorId, ct))
                return ProdutoErrors.FornecedorInexistente(request.FornecedorId);

            produto.Atualizar(request.Nome, request.Descricao, request.Preco, request.FornecedorId);
            await repository.UpdateAsync(produto, ct);

            var atualizado = await repository.GetByIdAsync(id, ct);
            return OperationResult<ProdutoResponse>.Success(atualizado.ToResponse());
        }
        catch (DomainException ex)
        {
            return ex.Error;
        }
    }

    public async Task<OperationResult<ProdutoResponse>> AjustarEstoqueAsync(int id, int quantidade, CancellationToken ct = default)
    {
        try
        {
            var produto = await repository.GetForUpdateAsync(id, ct);
            produto.AjustarEstoque(quantidade);
            await repository.UpdateAsync(produto, ct);

            var atualizado = await repository.GetByIdAsync(id, ct);
            return OperationResult<ProdutoResponse>.Success(atualizado.ToResponse());
        }
        catch (DomainException ex)
        {
            return ex.Error;
        }
    }

    public async Task<OperationResult> DeleteAsync(int id, CancellationToken ct = default)
    {
        try
        {
            await repository.DeleteAsync(id, ct);
            return OperationResult.Success();
        }
        catch (DomainException ex)
        {
            return ex.Error;
        }
    }
}
