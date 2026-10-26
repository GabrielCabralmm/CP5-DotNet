using Estoque.API.Hateoas;
using Estoque.API.Security;
using Estoque.Application.DTOs;
using Estoque.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Estoque.API.Controllers;

[Route("api/v1/cep")]
[Authorize(Policy = Politicas.Leitura)]
public sealed class CepController(ICepService cepService) : ApiControllerBase
{
    [HttpGet("{cep}", Name = nameof(ConsultarCep))]
    [ProducesResponseType(typeof(Resource<EnderecoCepResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status502BadGateway)]
    public async Task<IActionResult> ConsultarCep(string cep, CancellationToken ct)
    {
        var result = await cepService.ConsultarAsync(cep, ct);
        if (result.IsFailure)
            return FromError(result.Error!);

        var links = new List<Link>
        {
            LinkTo(nameof(ConsultarCep), new { cep = result.Data!.Cep }, "self", "GET"),
            LinkTo(nameof(FornecedoresController.ListarFornecedores), new { uf = result.Data!.Uf }, "fornecedores-na-uf", "GET")
        };

        if (IsAdmin)
            links.Add(LinkTo(nameof(FornecedoresController.CriarFornecedor), null, "criar-fornecedor", "POST"));

        return Ok(new Resource<EnderecoCepResponse>(result.Data!, links));
    }
}
