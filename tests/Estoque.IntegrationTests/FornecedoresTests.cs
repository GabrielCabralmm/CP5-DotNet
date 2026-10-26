using System.Net;
using System.Net.Http.Json;
using Estoque.IntegrationTests.Infra;

namespace Estoque.IntegrationTests;

[Collection(ApiCollection.Nome)]
public sealed class FornecedoresTests(EstoqueApiFactory factory)
{
    [Fact]
    public async Task CicloCompleto_CriaConsultaAtualizaERemoveNoBancoDaAplicacao()
    {
        var admin = await factory.CriarClienteAdminAsync();
        var cnpj = TestHelpers.NovoCnpj();

        var id = await admin.CriarFornecedorAsync(cnpj);

        var consulta = await admin.GetAsync($"/api/v1/fornecedores/{id}");
        var json = await consulta.LerJsonAsync();
        Assert.Equal(HttpStatusCode.OK, consulta.StatusCode);
        Assert.Equal("São Paulo", json.GetProperty("data").GetProperty("cidade").GetString());
        Assert.Contains("self", json.Rels());
        Assert.Contains("update", json.Rels());
        Assert.Contains("delete", json.Rels());

        var atualizacao = await admin.PutAsJsonAsync($"/api/v1/fornecedores/{id}", new
        {
            razaoSocial = "Fornecedor Atualizado LTDA",
            email = "atualizado@teste.com.br",
            telefone = "(11) 5000-0000",
            cep = "01538-001"
        });
        var atualizado = await atualizacao.LerJsonAsync();
        Assert.Equal(HttpStatusCode.OK, atualizacao.StatusCode);
        Assert.Equal("Fornecedor Atualizado LTDA", atualizado.GetProperty("data").GetProperty("razaoSocial").GetString());

        var remocao = await admin.DeleteAsync($"/api/v1/fornecedores/{id}");
        Assert.Equal(HttpStatusCode.NoContent, remocao.StatusCode);

        var aposRemocao = await admin.GetAsync($"/api/v1/fornecedores/{id}");
        var erro = await aposRemocao.LerJsonAsync();
        Assert.Equal(HttpStatusCode.NotFound, aposRemocao.StatusCode);
        Assert.Equal("Fornecedor.NaoEncontrado", erro.GetProperty("code").GetString());
    }

    [Fact]
    public async Task Criar_ComCnpjDuplicado_Retorna409()
    {
        var admin = await factory.CriarClienteAdminAsync();
        var cnpj = TestHelpers.NovoCnpj();
        var id = await admin.CriarFornecedorAsync(cnpj);

        try
        {
            var duplicado = await admin.PostAsJsonAsync("/api/v1/fornecedores", new
            {
                razaoSocial = "Duplicado LTDA",
                cnpj,
                email = "duplicado@teste.com.br",
                cep = "01538001"
            });
            var json = await duplicado.LerJsonAsync();

            Assert.Equal(HttpStatusCode.Conflict, duplicado.StatusCode);
            Assert.Equal("Fornecedor.CnpjDuplicado", json.GetProperty("code").GetString());
        }
        finally
        {
            await admin.DeleteAsync($"/api/v1/fornecedores/{id}");
        }
    }

    [Fact]
    public async Task Criar_ComCepInexistente_RetornaErroDoServicoExterno()
    {
        var admin = await factory.CriarClienteAdminAsync();

        var response = await admin.PostAsJsonAsync("/api/v1/fornecedores", new
        {
            razaoSocial = "Fornecedor Sem CEP",
            cnpj = TestHelpers.NovoCnpj(),
            email = "semcep@teste.com.br",
            cep = CepServiceDeTeste.CepInexistente
        });
        var json = await response.LerJsonAsync();

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        Assert.Equal("Cep.NaoEncontrado", json.GetProperty("code").GetString());
    }

    [Fact]
    public async Task Remover_ComProdutosVinculados_Retorna409RetornadoPeloRepositorio()
    {
        var admin = await factory.CriarClienteAdminAsync();
        var fornecedorId = await admin.CriarFornecedorAsync();
        var produtoId = await admin.CriarProdutoAsync(fornecedorId);

        try
        {
            var response = await admin.DeleteAsync($"/api/v1/fornecedores/{fornecedorId}");
            var json = await response.LerJsonAsync();

            Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
            Assert.Equal("Fornecedor.PossuiProdutos", json.GetProperty("code").GetString());
        }
        finally
        {
            await admin.DeleteAsync($"/api/v1/produtos/{produtoId}");
            await admin.DeleteAsync($"/api/v1/fornecedores/{fornecedorId}");
        }
    }

    [Fact]
    public async Task Listar_ComPaginacao_RetornaPageResultComLinksHateoas()
    {
        var leitor = await factory.CriarClienteLeitorAsync();

        var response = await leitor.GetAsync("/api/v1/fornecedores?pageNumber=1&pageSize=2");
        var json = await response.LerJsonAsync();

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal(1, json.GetProperty("pageNumber").GetInt32());
        Assert.Equal(2, json.GetProperty("pageSize").GetInt32());
        Assert.True(json.GetProperty("totalItems").GetInt32() >= 3);
        Assert.True(json.GetProperty("items").GetArrayLength() <= 2);
        Assert.Contains("self", json.Rels());
        Assert.Contains("next", json.Rels());

        var primeiroItem = json.GetProperty("items")[0];
        Assert.Contains("self", primeiroItem.Rels());
        Assert.DoesNotContain("delete", primeiroItem.Rels());
    }
}
