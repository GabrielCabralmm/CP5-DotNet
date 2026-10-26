using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using Estoque.Application.DTOs;
using Estoque.Application.Interfaces;
using Estoque.Domain.Common;
using Estoque.Domain.Errors;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Estoque.IntegrationTests.Infra;

public sealed class EstoqueApiFactory : WebApplicationFactory<Program>
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Development");
        builder.ConfigureTestServices(services =>
        {
            services.RemoveAll<ICepService>();
            services.AddSingleton<ICepService, CepServiceDeTeste>();
        });
    }

    public async Task<HttpClient> CriarClienteAutenticadoAsync(string username, string password)
    {
        var client = CreateClient();
        var response = await client.PostAsJsonAsync("/api/v1/auth/login", new { username, password });
        response.EnsureSuccessStatusCode();

        var json = await response.Content.ReadFromJsonAsync<JsonElement>();
        var token = json.GetProperty("data").GetProperty("accessToken").GetString();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        return client;
    }

    public Task<HttpClient> CriarClienteAdminAsync() => CriarClienteAutenticadoAsync("admin", "Admin@123");

    public Task<HttpClient> CriarClienteLeitorAsync() => CriarClienteAutenticadoAsync("leitor", "Leitor@123");
}

public sealed class CepServiceDeTeste : ICepService
{
    public const string CepInexistente = "99999999";

    public Task<OperationResult<EnderecoCepResponse>> ConsultarAsync(string cep, CancellationToken ct = default)
    {
        var digits = Guard.OnlyDigits(cep);
        OperationResult<EnderecoCepResponse> result = digits switch
        {
            { Length: not 8 } => CepErrors.Invalido(cep),
            CepInexistente => CepErrors.NaoEncontrado(digits),
            _ => OperationResult<EnderecoCepResponse>.Success(new EnderecoCepResponse(digits, "Avenida Lins de Vasconcelos", "Aclimação", "São Paulo", "SP"))
        };

        return Task.FromResult(result);
    }
}

[CollectionDefinition(Nome)]
public sealed class ApiCollection : ICollectionFixture<EstoqueApiFactory>
{
    public const string Nome = "API com banco da aplicação";
}

public static class TestHelpers
{
    public static string NovoCnpj() =>
        string.Concat(Enumerable.Range(0, 14).Select(_ => Random.Shared.Next(0, 10)));

    public static string NovoSku() => $"TST-{Guid.NewGuid():N}"[..20].ToUpperInvariant();

    public static async Task<JsonElement> LerJsonAsync(this HttpResponseMessage response) =>
        await response.Content.ReadFromJsonAsync<JsonElement>();

    public static IEnumerable<string?> Rels(this JsonElement element) =>
        element.GetProperty("links").EnumerateArray().Select(l => l.GetProperty("rel").GetString());

    public static async Task<int> CriarFornecedorAsync(this HttpClient admin, string? cnpj = null)
    {
        var response = await admin.PostAsJsonAsync("/api/v1/fornecedores", new
        {
            razaoSocial = "Fornecedor Integração LTDA",
            cnpj = cnpj ?? NovoCnpj(),
            email = "integracao@teste.com.br",
            telefone = "(11) 4000-0000",
            cep = "01538-001"
        });

        response.EnsureSuccessStatusCode();
        var json = await response.LerJsonAsync();
        return json.GetProperty("data").GetProperty("id").GetInt32();
    }

    public static async Task<int> CriarProdutoAsync(this HttpClient admin, int fornecedorId, int quantidade = 10)
    {
        var response = await admin.PostAsJsonAsync("/api/v1/produtos", new
        {
            nome = "Produto Integração",
            descricao = "Criado pelos testes funcionais",
            sku = NovoSku(),
            preco = 49.90m,
            quantidadeEstoque = quantidade,
            fornecedorId
        });

        response.EnsureSuccessStatusCode();
        var json = await response.LerJsonAsync();
        return json.GetProperty("data").GetProperty("id").GetInt32();
    }
}
