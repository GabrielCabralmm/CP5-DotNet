using Estoque.API.Hateoas;
using Estoque.API.Security;
using Estoque.Application.DTOs;
using Estoque.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Estoque.API.Controllers;

[Route("api/v1/produtos")]
[Authorize(Policy = Politicas.Leitura)]
public sealed class ProdutosController(IProdutoService service) : ApiControllerBase
{
    [HttpGet(Name = nameof(ListarProdutos))]
    [ProducesResponseType(typeof(PagedResource<ProdutoResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> ListarProdutos(
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] int? fornecedorId = null,
        [FromQuery] string? nome = null,
        CancellationToken ct = default)
    {
        var result = await service.GetPagedAsync(pageNumber, pageSize, fornecedorId, nome, ct);
        if (result.IsFailure)
            return FromError(result.Error!);

        return Ok(ToPagedResource(result.Data!, Links, nameof(ListarProdutos), new { fornecedorId, nome }));
    }

    [HttpGet("{id:int}", Name = nameof(ObterProduto))]
    [ProducesResponseType(typeof(Resource<ProdutoResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ObterProduto(int id, CancellationToken ct)
    {
        var result = await service.GetByIdAsync(id, ct);
        return result.IsSuccess
            ? Ok(new Resource<ProdutoResponse>(result.Data!, Links(result.Data!)))
            : FromError(result.Error!);
    }

    [HttpPost(Name = nameof(CriarProduto))]
    [Authorize(Policy = Politicas.Admin)]
    [ProducesResponseType(typeof(Resource<ProdutoResponse>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> CriarProduto([FromBody] CreateProdutoRequest request, CancellationToken ct)
    {
        var result = await service.CreateAsync(request, ct);
        if (result.IsFailure)
            return FromError(result.Error!);

        var criado = result.Data!;
        return CreatedAtRoute(nameof(ObterProduto), new { id = criado.Id }, new Resource<ProdutoResponse>(criado, Links(criado)));
    }

    [HttpPut("{id:int}", Name = nameof(AtualizarProduto))]
    [Authorize(Policy = Politicas.Admin)]
    [ProducesResponseType(typeof(Resource<ProdutoResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> AtualizarProduto(int id, [FromBody] UpdateProdutoRequest request, CancellationToken ct)
    {
        var result = await service.UpdateAsync(id, request, ct);
        return result.IsSuccess
            ? Ok(new Resource<ProdutoResponse>(result.Data!, Links(result.Data!)))
            : FromError(result.Error!);
    }

    [HttpPatch("{id:int}/estoque", Name = nameof(AjustarEstoque))]
    [Authorize(Policy = Politicas.Admin)]
    [ProducesResponseType(typeof(Resource<ProdutoResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> AjustarEstoque(int id, [FromBody] AjusteEstoqueRequest request, CancellationToken ct)
    {
        var result = await service.AjustarEstoqueAsync(id, request.Quantidade, ct);
        return result.IsSuccess
            ? Ok(new Resource<ProdutoResponse>(result.Data!, Links(result.Data!)))
            : FromError(result.Error!);
    }

    [HttpDelete("{id:int}", Name = nameof(RemoverProduto))]
    [Authorize(Policy = Politicas.Admin)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> RemoverProduto(int id, CancellationToken ct)
    {
        var result = await service.DeleteAsync(id, ct);
        return result.IsSuccess ? NoContent() : FromError(result.Error!);
    }

    private IReadOnlyList<Link> Links(ProdutoResponse produto)
    {
        var links = new List<Link>
        {
            LinkTo(nameof(ObterProduto), new { id = produto.Id }, "self", "GET"),
            LinkTo(nameof(FornecedoresController.ObterFornecedor), new { id = produto.FornecedorId }, "fornecedor", "GET")
        };

        if (IsAdmin)
        {
            links.Add(LinkTo(nameof(AtualizarProduto), new { id = produto.Id }, "update", "PUT"));
            links.Add(LinkTo(nameof(AjustarEstoque), new { id = produto.Id }, "ajustar-estoque", "PATCH"));
            links.Add(LinkTo(nameof(RemoverProduto), new { id = produto.Id }, "delete", "DELETE"));
        }

        return links;
    }
}
