using System.Net;
using System.Net.Http.Json;
using Estoque.IntegrationTests.Infra;

namespace Estoque.IntegrationTests;

[Collection(ApiCollection.Nome)]
public sealed class ProdutosTests(EstoqueApiFactory factory)
{
    [Fact]
    public async Task AjusteDeEstoque_AplicaRegraDeNegocio()
    {
        var admin = await factory.CriarClienteAdminAsync();
        var fornecedorId = await admin.CriarFornecedorAsync();
        var produtoId = await admin.CriarProdutoAsync(fornecedorId, quantidade: 5);

        try
        {
            var entrada = await admin.PatchAsJsonAsync($"/api/v1/produtos/{produtoId}/estoque", new { quantidade = 10 });
            var entradaJson = await entrada.LerJsonAsync();
            Assert.Equal(HttpStatusCode.OK, entrada.StatusCode);
            Assert.Equal(15, entradaJson.GetProperty("data").GetProperty("quantidadeEstoque").GetInt32());

            var saidaInvalida = await admin.PatchAsJsonAsync($"/api/v1/produtos/{produtoId}/estoque", new { quantidade = -100 });
            var erro = await saidaInvalida.LerJsonAsync();
            Assert.Equal(HttpStatusCode.Conflict, saidaInvalida.StatusCode);
            Assert.Equal("Produto.EstoqueInsuficiente", erro.GetProperty("code").GetString());
        }
        finally
        {
            await admin.DeleteAsync($"/api/v1/produtos/{produtoId}");
            await admin.DeleteAsync($"/api/v1/fornecedores/{fornecedorId}");
        }
    }

    [Fact]
    public async Task Remover_DuasVezes_SegundaRetorna404LancadoPeloRepositorio()
    {
        var admin = await factory.CriarClienteAdminAsync();
        var fornecedorId = await admin.CriarFornecedorAsync();
        var produtoId = await admin.CriarProdutoAsync(fornecedorId);

        try
        {
            var primeira = await admin.DeleteAsync($"/api/v1/produtos/{produtoId}");
            var segunda = await admin.DeleteAsync($"/api/v1/produtos/{produtoId}");
            var json = await segunda.LerJsonAsync();

            Assert.Equal(HttpStatusCode.NoContent, primeira.StatusCode);
            Assert.Equal(HttpStatusCode.NotFound, segunda.StatusCode);
            Assert.Equal("Produto.NaoEncontrado", json.GetProperty("code").GetString());
        }
        finally
        {
            await admin.DeleteAsync($"/api/v1/fornecedores/{fornecedorId}");
        }
    }

    [Fact]
    public async Task Criar_ComFornecedorInexistente_Retorna400()
    {
        var admin = await factory.CriarClienteAdminAsync();

        var response = await admin.PostAsJsonAsync("/api/v1/produtos", new
        {
            nome = "Produto Órfão",
            sku = TestHelpers.NovoSku(),
            preco = 10.0m,
            quantidadeEstoque = 1,
            fornecedorId = int.MaxValue
        });
        var json = await response.LerJsonAsync();

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Equal("Produto.FornecedorInexistente", json.GetProperty("code").GetString());
    }

    [Fact]
    public async Task Criar_ComPayloadInvalido_Retorna400DeValidacao()
    {
        var admin = await factory.CriarClienteAdminAsync();

        var response = await admin.PostAsJsonAsync("/api/v1/produtos", new
        {
            nome = "",
            sku = "X",
            preco = 0m,
            quantidadeEstoque = -1,
            fornecedorId = 0
        });

        var json = await response.LerJsonAsync();

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Equal("Requisicao.Invalida", json.GetProperty("code").GetString());
        Assert.True(json.GetProperty("errors").TryGetProperty("Preco", out _));
    }

    [Fact]
    public async Task ListarPorFornecedor_PaginaComLinksDeNavegacao()
    {
        var admin = await factory.CriarClienteAdminAsync();
        var fornecedorId = await admin.CriarFornecedorAsync();
        var ids = new List<int>();

        try
        {
            for (var i = 0; i < 3; i++)
                ids.Add(await admin.CriarProdutoAsync(fornecedorId));

            var pagina1 = await admin.GetAsync($"/api/v1/produtos?fornecedorId={fornecedorId}&pageNumber=1&pageSize=2");
            var json1 = await pagina1.LerJsonAsync();
            Assert.Equal(HttpStatusCode.OK, pagina1.StatusCode);
            Assert.Equal(3, json1.GetProperty("totalItems").GetInt32());
            Assert.Equal(2, json1.GetProperty("totalPages").GetInt32());
            Assert.True(json1.GetProperty("hasNext").GetBoolean());
            Assert.Contains("next", json1.Rels());
            Assert.Contains("ajustar-estoque", json1.GetProperty("items")[0].Rels());

            var pagina2 = await admin.GetAsync($"/api/v1/produtos?fornecedorId={fornecedorId}&pageNumber=2&pageSize=2");
            var json2 = await pagina2.LerJsonAsync();
            Assert.Equal(1, json2.GetProperty("items").GetArrayLength());
            Assert.Contains("previous", json2.Rels());
            Assert.DoesNotContain("next", json2.Rels());
        }
        finally
        {
            foreach (var id in ids)
                await admin.DeleteAsync($"/api/v1/produtos/{id}");
            await admin.DeleteAsync($"/api/v1/fornecedores/{fornecedorId}");
        }
    }
}
