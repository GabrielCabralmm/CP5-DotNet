using Estoque.API.Hateoas;
using Estoque.Application.DTOs;
using Estoque.Application.Interfaces;
using Estoque.Infrastructure.Security;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Estoque.API.Controllers;

[Route("api/v1/auth")]
public sealed class AuthController(IAuthService authService) : ApiControllerBase
{
    [AllowAnonymous]
    [HttpPost("login", Name = nameof(Login))]
    [ProducesResponseType(typeof(Resource<TokenResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Login([FromBody] LoginRequest request, CancellationToken ct)
    {
        var result = await authService.LoginAsync(request, ct);
        if (result.IsFailure)
            return FromError(result.Error!);

        var links = new List<Link>
        {
            LinkTo(nameof(Me), null, "me", "GET"),
            LinkTo(nameof(FornecedoresController.ListarFornecedores), null, "fornecedores", "GET"),
            LinkTo(nameof(ProdutosController.ListarProdutos), null, "produtos", "GET")
        };

        return Ok(new Resource<TokenResponse>(result.Data!, links));
    }

    [Authorize]
    [HttpGet("me", Name = nameof(Me))]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    public IActionResult Me()
    {
        var dados = new
        {
            Username = User.FindFirst(JwtTokenService.NameClaim)?.Value,
            Perfil = User.FindFirst(JwtTokenService.RoleClaim)?.Value,
            Claims = User.Claims.Select(c => new { c.Type, c.Value })
        };

        return Ok(new Resource<object>(dados, new[] { LinkTo(nameof(Me), null, "self", "GET") }));
    }
}
