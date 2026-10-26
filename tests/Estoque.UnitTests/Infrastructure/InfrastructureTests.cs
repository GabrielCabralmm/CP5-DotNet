using System.IdentityModel.Tokens.Jwt;
using System.Net;
using Estoque.Domain.Common;
using Estoque.Domain.Entities;
using Estoque.Infrastructure.ExternalServices;
using Estoque.Infrastructure.Security;
using Estoque.UnitTests.Fakes;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;

namespace Estoque.UnitTests.Infrastructure;

public sealed class ViaCepServiceTests
{
    private const string RespostaSe = """
        {"cep":"01001-000","logradouro":"Praça da Sé","complemento":"lado ímpar","bairro":"Sé","localidade":"São Paulo","uf":"SP"}
        """;

    private static ViaCepService Criar(HttpMessageHandler handler, IMemoryCache? cache = null) =>
        new(new HttpClient(handler) { BaseAddress = new Uri("https://viacep.com.br/ws/") },
            cache ?? new MemoryCache(new MemoryCacheOptions()),
            NullLogger<ViaCepService>.Instance);

    [Fact]
    public async Task ConsultarAsync_CepValido_RetornaEndereco()
    {
        var service = Criar(new StubHttpMessageHandler(HttpStatusCode.OK, RespostaSe));

        var result = await service.ConsultarAsync("01001-000");

        Assert.True(result.IsSuccess);
        Assert.Equal("01001000", result.Data!.Cep);
        Assert.Equal("São Paulo", result.Data.Cidade);
        Assert.Equal("SP", result.Data.Uf);
    }

    [Fact]
    public async Task ConsultarAsync_RespostaComErro_RetornaNotFound()
    {
        var service = Criar(new StubHttpMessageHandler(HttpStatusCode.OK, """{"erro":"true"}"""));

        var result = await service.ConsultarAsync("99999999");

        Assert.True(result.IsFailure);
        Assert.Equal("Cep.NaoEncontrado", result.Error!.Code);
    }

    [Fact]
    public async Task ConsultarAsync_FormatoInvalido_NaoChamaServicoExterno()
    {
        var handler = new StubHttpMessageHandler(HttpStatusCode.OK, RespostaSe);
        var service = Criar(handler);

        var result = await service.ConsultarAsync("123");

        Assert.Equal("Cep.Invalido", result.Error!.Code);
        Assert.Equal(0, handler.Chamadas);
    }

    [Fact]
    public async Task ConsultarAsync_SegundaConsulta_UsaCache()
    {
        var handler = new StubHttpMessageHandler(HttpStatusCode.OK, RespostaSe);
        var service = Criar(handler);

        await service.ConsultarAsync("01001000");
        var segunda = await service.ConsultarAsync("01001-000");

        Assert.True(segunda.IsSuccess);
        Assert.Equal(1, handler.Chamadas);
    }

    [Fact]
    public async Task ConsultarAsync_ServicoForaDoAr_RetornaErroDeServicoExterno()
    {
        var service = Criar(new FailingHttpMessageHandler());

        var result = await service.ConsultarAsync("01001000");

        Assert.True(result.IsFailure);
        Assert.Equal(ErrorType.ExternalService, result.Error!.Type);
    }

    [Fact]
    public async Task ConsultarAsync_Status500_RetornaErroDeServicoExterno()
    {
        var service = Criar(new StubHttpMessageHandler(HttpStatusCode.InternalServerError, "{}"));

        var result = await service.ConsultarAsync("01001000");

        Assert.Equal("Cep.ServicoIndisponivel", result.Error!.Code);
    }
}

public sealed class SecurityTests
{
    [Fact]
    public void PasswordHasher_VerificaSenhaCorretaERejeitaIncorreta()
    {
        var hasher = new Pbkdf2PasswordHasher();
        var hash = hasher.Hash("Senha@123");

        Assert.True(hasher.Verify("Senha@123", hash));
        Assert.False(hasher.Verify("outra", hash));
        Assert.False(hasher.Verify("Senha@123", "hash-invalido"));
    }

    [Fact]
    public void JwtTokenService_GeraTokenComPerfilEUsuario()
    {
        var settings = new JwtSettings
        {
            Issuer = "Estoque.API",
            Audience = "Estoque.Clients",
            SecretKey = "chave-de-teste-com-mais-de-trinta-e-dois-caracteres",
            ExpirationMinutes = 30
        };
        var service = new JwtTokenService(Options.Create(settings));
        var usuario = TestData.ComId(new Usuario("admin", "hash", Perfis.Admin), 1);

        var token = service.GerarToken(usuario);
        var jwt = new JwtSecurityTokenHandler().ReadJwtToken(token.AccessToken);

        Assert.Equal("Bearer", token.TokenType);
        Assert.Equal("Estoque.API", jwt.Issuer);
        Assert.Contains(jwt.Claims, c => c.Type == JwtTokenService.RoleClaim && c.Value == Perfis.Admin);
        Assert.Contains(jwt.Claims, c => c.Type == JwtTokenService.NameClaim && c.Value == "admin");
    }
}
