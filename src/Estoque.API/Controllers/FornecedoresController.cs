using Estoque.API.Hateoas;
using Estoque.API.Security;
using Estoque.Application.DTOs;
using Estoque.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Estoque.API.Controllers;

[Route("api/v1/fornecedores")]
[Authorize(Policy = Politicas.Leitura)]
public sealed class FornecedoresController(IFornecedorService service) : ApiControllerBase
{
    [HttpGet(Name = nameof(ListarFornecedores))]
    [ProducesResponseType(typeof(PagedResource<FornecedorResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> ListarFornecedores(
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] string? uf = null,
        CancellationToken ct = default)
    {
        var result = await service.GetPagedAsync(pageNumber, pageSize, uf, ct);
        if (result.IsFailure)
            return FromError(result.Error!);

        return Ok(ToPagedResource(result.Data!, f => Links(f.Id), nameof(ListarFornecedores), new { uf }));
    }

    [HttpGet("{id:int}", Name = nameof(ObterFornecedor))]
    [ProducesResponseType(typeof(Resource<FornecedorResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ObterFornecedor(int id, CancellationToken ct)
    {
        var result = await service.GetByIdAsync(id, ct);
        return result.IsSuccess
            ? Ok(new Resource<FornecedorResponse>(result.Data!, Links(id)))
            : FromError(result.Error!);
    }

    [HttpPost(Name = nameof(CriarFornecedor))]
    [Authorize(Policy = Politicas.Admin)]
    [ProducesResponseType(typeof(Resource<FornecedorResponse>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status502BadGateway)]
    public async Task<IActionResult> CriarFornecedor([FromBody] CreateFornecedorRequest request, CancellationToken ct)
    {
        var result = await service.CreateAsync(request, ct);
        if (result.IsFailure)
            return FromError(result.Error!);

        var criado = result.Data!;
        return CreatedAtRoute(nameof(ObterFornecedor), new { id = criado.Id }, new Resource<FornecedorResponse>(criado, Links(criado.Id)));
    }

    [HttpPut("{id:int}", Name = nameof(AtualizarFornecedor))]
    [Authorize(Policy = Politicas.Admin)]
    [ProducesResponseType(typeof(Resource<FornecedorResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> AtualizarFornecedor(int id, [FromBody] UpdateFornecedorRequest request, CancellationToken ct)
    {
        var result = await service.UpdateAsync(id, request, ct);
        return result.IsSuccess
            ? Ok(new Resource<FornecedorResponse>(result.Data!, Links(id)))
            : FromError(result.Error!);
    }

    [HttpDelete("{id:int}", Name = nameof(RemoverFornecedor))]
    [Authorize(Policy = Politicas.Admin)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> RemoverFornecedor(int id, CancellationToken ct)
    {
        var result = await service.DeleteAsync(id, ct);
        return result.IsSuccess ? NoContent() : FromError(result.Error!);
    }

    private IReadOnlyList<Link> Links(int id)
    {
        var links = new List<Link>
        {
            LinkTo(nameof(ObterFornecedor), new { id }, "self", "GET"),
            LinkTo(nameof(ProdutosController.ListarProdutos), new { fornecedorId = id }, "produtos", "GET")
        };

        if (IsAdmin)
        {
            links.Add(LinkTo(nameof(AtualizarFornecedor), new { id }, "update", "PUT"));
            links.Add(LinkTo(nameof(RemoverFornecedor), new { id }, "delete", "DELETE"));
        }

        return links;
    }
}
