using System.Net;
using System.Net.Http.Json;
using Estoque.IntegrationTests.Infra;

namespace Estoque.IntegrationTests;

[Collection(ApiCollection.Nome)]
public sealed class AuthTests(EstoqueApiFactory factory)
{
    [Fact]
    public async Task Login_ComAdminValido_RetornaTokenJwt()
    {
        var client = factory.CreateClient();

        var response = await client.PostAsJsonAsync("/api/v1/auth/login", new { username = "admin", password = "Admin@123" });
        var json = await response.LerJsonAsync();

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.False(string.IsNullOrWhiteSpace(json.GetProperty("data").GetProperty("accessToken").GetString()));
        Assert.Equal("Admin", json.GetProperty("data").GetProperty("perfil").GetString());
        Assert.Contains("fornecedores", json.Rels());
    }

    [Fact]
    public async Task Login_ComSenhaInvalida_Retorna401ComErroCustomizado()
    {
        var client = factory.CreateClient();

        var response = await client.PostAsJsonAsync("/api/v1/auth/login", new { username = "admin", password = "senha-errada" });
        var json = await response.LerJsonAsync();

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        Assert.Equal("Auth.CredenciaisInvalidas", json.GetProperty("code").GetString());
        Assert.False(json.GetProperty("success").GetBoolean());
    }

    [Fact]
    public async Task EndpointProtegido_SemToken_Retorna401()
    {
        var client = factory.CreateClient();

        var response = await client.GetAsync("/api/v1/fornecedores");
        var json = await response.LerJsonAsync();

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        Assert.Equal("Auth.NaoAutenticado", json.GetProperty("code").GetString());
    }

    [Fact]
    public async Task Leitor_PodeConsultarMasNaoPodeCriar()
    {
        var leitor = await factory.CriarClienteLeitorAsync();

        var consulta = await leitor.GetAsync("/api/v1/produtos");
        var criacao = await leitor.PostAsJsonAsync("/api/v1/fornecedores", new
        {
            razaoSocial = "Tentativa Leitor",
            cnpj = TestHelpers.NovoCnpj(),
            email = "leitor@teste.com.br",
            cep = "01538001"
        });
        var json = await criacao.LerJsonAsync();

        Assert.Equal(HttpStatusCode.OK, consulta.StatusCode);
        Assert.Equal(HttpStatusCode.Forbidden, criacao.StatusCode);
        Assert.Equal("Auth.AcessoNegado", json.GetProperty("code").GetString());
    }
}
