using Estoque.Application.DTOs;
using Estoque.Application.Interfaces;
using Estoque.Application.Mappings;
using Estoque.Domain.Common;
using Estoque.Domain.Entities;
using Estoque.Domain.Exceptions;
using Estoque.Domain.Interfaces;
using Estoque.Domain.ValueObjects;

namespace Estoque.Application.Services;

public sealed class FornecedorService(IFornecedorRepository repository, ICepService cepService) : IFornecedorService
{
    public async Task<OperationResult<PageResult<FornecedorResponse>>> GetPagedAsync(int pageNumber, int pageSize, string? uf, CancellationToken ct = default)
    {
        var page = await repository.GetPagedAsync(PageRequest.Create(pageNumber, pageSize), uf, ct);
        return OperationResult<PageResult<FornecedorResponse>>.Success(page.Map(f => f.ToResponse()));
    }

    public async Task<OperationResult<FornecedorResponse>> GetByIdAsync(int id, CancellationToken ct = default)
    {
        var result = await repository.GetByIdAsync(id, ct);
        return result.Map(f => f.ToResponse());
    }

    public async Task<OperationResult<FornecedorResponse>> CreateAsync(CreateFornecedorRequest request, CancellationToken ct = default)
    {
        try
        {
            var cep = await cepService.ConsultarAsync(request.Cep, ct);
            if (cep.IsFailure)
                return cep.Error!;

            var fornecedor = new Fornecedor(
                request.RazaoSocial,
                request.Cnpj,
                request.Email,
                request.Telefone,
                cep.Data!.ToEndereco());

            var criado = await repository.AddAsync(fornecedor, ct);
            return OperationResult<FornecedorResponse>.Success(criado.ToResponse());
        }
        catch (DomainException ex)
        {
            return ex.Error;
        }
    }

    public async Task<OperationResult<FornecedorResponse>> UpdateAsync(int id, UpdateFornecedorRequest request, CancellationToken ct = default)
    {
        try
        {
            var atual = await repository.GetByIdAsync(id, ct);
            if (atual.IsFailure)
                return atual.Error!;

            var fornecedor = atual.Data!;
            Endereco? novoEndereco = null;

            if (Guard.OnlyDigits(request.Cep) != fornecedor.Cep)
            {
                var cep = await cepService.ConsultarAsync(request.Cep, ct);
                if (cep.IsFailure)
                    return cep.Error!;

                novoEndereco = cep.Data!.ToEndereco();
            }

            fornecedor.Atualizar(request.RazaoSocial, request.Email, request.Telefone, novoEndereco);

            var update = await repository.UpdateAsync(fornecedor, ct);
            return update.IsSuccess
                ? OperationResult<FornecedorResponse>.Success(fornecedor.ToResponse())
                : update.Error!;
        }
        catch (DomainException ex)
        {
            return ex.Error;
        }
    }

    public Task<OperationResult> DeleteAsync(int id, CancellationToken ct = default) =>
        repository.DeleteAsync(id, ct);
}
