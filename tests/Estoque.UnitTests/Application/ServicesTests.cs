using Estoque.Application.DTOs;
using Estoque.Application.Services;
using Estoque.Domain.Common;
using Estoque.Domain.Entities;
using Estoque.Domain.Errors;
using Estoque.Infrastructure.Security;
using Estoque.UnitTests.Fakes;

namespace Estoque.UnitTests.Application;

public sealed class FornecedorServiceTests
{
    private readonly FakeFornecedorRepository _repository = new();
    private readonly FakeCepService _cep = new();
    private readonly FornecedorService _service;

    public FornecedorServiceTests()
    {
        _service = new FornecedorService(_repository, _cep);
    }

    private static CreateFornecedorRequest Request(string cnpj = "11222333000181") =>
        new("Fornecedor Teste", cnpj, "teste@fornecedor.com.br", null, "01001-000");

    [Fact]
    public async Task CreateAsync_ComDadosValidos_RetornaSucessoComEnderecoDoServicoExterno()
    {
        var result = await _service.CreateAsync(Request());

        Assert.True(result.IsSuccess);
        Assert.Equal("São Paulo", result.Data!.Cidade);
        Assert.Equal("01001000", result.Data.Cep);
        Assert.Single(_repository.Itens);
    }

    [Fact]
    public async Task CreateAsync_QuandoCepNaoExiste_RetornaFalhaSemPersistir()
    {
        _cep.ErroForcado = CepErrors.NaoEncontrado("99999999");

        var result = await _service.CreateAsync(Request());

        Assert.True(result.IsFailure);
        Assert.Equal(ErrorType.NotFound, result.Error!.Type);
        Assert.Empty(_repository.Itens);
    }

    [Fact]
    public async Task CreateAsync_QuandoRepositorioLancaDuplicado_RetornaConflito()
    {
        _repository.LancarDuplicado = true;

        var result = await _service.CreateAsync(Request());

        Assert.True(result.IsFailure);
        Assert.Equal("Fornecedor.CnpjDuplicado", result.Error!.Code);
        Assert.Equal(ErrorType.Conflict, result.Error.Type);
    }

    [Fact]
    public async Task CreateAsync_ComCnpjInvalido_RetornaErroDeValidacao()
    {
        var result = await _service.CreateAsync(Request("123"));

        Assert.True(result.IsFailure);
        Assert.Equal(ErrorType.Validation, result.Error!.Type);
    }

    [Fact]
    public async Task GetByIdAsync_Inexistente_RetornaErroRetornadoPeloRepositorio()
    {
        var result = await _service.GetByIdAsync(99);

        Assert.True(result.IsFailure);
        Assert.Equal("Fornecedor.NaoEncontrado", result.Error!.Code);
    }

    [Fact]
    public async Task UpdateAsync_ComMesmoCep_NaoConsultaServicoExterno()
    {
        _repository.Itens.Add(TestData.Fornecedor(1));

        var result = await _service.UpdateAsync(1, new UpdateFornecedorRequest("Nome Atualizado", "novo@x.com.br", null, "01310-100"));

        Assert.True(result.IsSuccess);
        Assert.Equal("Nome Atualizado", result.Data!.RazaoSocial);
        Assert.Equal(0, _cep.Chamadas);
    }

    [Fact]
    public async Task GetPagedAsync_RetornaMetadadosDePaginacao()
    {
        for (var i = 1; i <= 5; i++)
            _repository.Itens.Add(TestData.Fornecedor(i));

        var result = await _service.GetPagedAsync(2, 2, null);

        Assert.True(result.IsSuccess);
        Assert.Equal(5, result.Data!.TotalItems);
        Assert.Equal(3, result.Data.TotalPages);
        Assert.Equal(2, result.Data.Items.Count);
        Assert.True(result.Data.HasNext);
        Assert.True(result.Data.HasPrevious);
    }
}

public sealed class ProdutoServiceTests
{
    private readonly FakeProdutoRepository _repository = new();
    private readonly FakeFornecedorRepository _fornecedores = new();
    private readonly ProdutoService _service;

    public ProdutoServiceTests()
    {
        _fornecedores.Itens.Add(TestData.Fornecedor(1));
        _service = new ProdutoService(_repository, _fornecedores);
    }

    [Fact]
    public async Task GetByIdAsync_QuandoRepositorioLancaNotFound_RetornaOperationResultDeFalha()
    {
        var result = await _service.GetByIdAsync(123);

        Assert.True(result.IsFailure);
        Assert.Equal("Produto.NaoEncontrado", result.Error!.Code);
        Assert.Equal(ErrorType.NotFound, result.Error.Type);
    }

    [Fact]
    public async Task CreateAsync_ComFornecedorInexistente_RetornaErroDeValidacao()
    {
        var request = new CreateProdutoRequest("Teclado", null, "SKU-X", 10m, 5, 999);

        var result = await _service.CreateAsync(request);

        Assert.True(result.IsFailure);
        Assert.Equal("Produto.FornecedorInexistente", result.Error!.Code);
    }

    [Fact]
    public async Task CreateAsync_ComDadosValidos_RetornaProdutoComNomeDoFornecedor()
    {
        var request = new CreateProdutoRequest("Teclado", "ABNT2", "sku-x", 150m, 5, 1);

        var result = await _service.CreateAsync(request);

        Assert.True(result.IsSuccess);
        Assert.Equal("SKU-X", result.Data!.Sku);
        Assert.Equal("Fornecedor Teste LTDA", result.Data.FornecedorNome);
    }

    [Fact]
    public async Task CreateAsync_ComSkuDuplicado_RetornaConflito()
    {
        _repository.Itens.Add(TestData.Produto(1));
        var request = new CreateProdutoRequest("Outro", null, "SKU-001", 10m, 1, 1);

        var result = await _service.CreateAsync(request);

        Assert.True(result.IsFailure);
        Assert.Equal("Produto.SkuDuplicado", result.Error!.Code);
    }

    [Fact]
    public async Task AjustarEstoqueAsync_SaidaMaiorQueSaldo_RetornaConflito()
    {
        _repository.Itens.Add(TestData.Produto(1, estoque: 2));

        var result = await _service.AjustarEstoqueAsync(1, -10);

        Assert.True(result.IsFailure);
        Assert.Equal("Produto.EstoqueInsuficiente", result.Error!.Code);
        Assert.Equal(ErrorType.Conflict, result.Error.Type);
    }

    [Fact]
    public async Task DeleteAsync_Inexistente_ConverteExcecaoDoRepositorioEmFalha()
    {
        var result = await _service.DeleteAsync(77);

        Assert.True(result.IsFailure);
        Assert.Equal(ErrorType.NotFound, result.Error!.Type);
    }
}

public sealed class AuthServiceTests
{
    private readonly Pbkdf2PasswordHasher _hasher = new();

    [Fact]
    public async Task LoginAsync_ComCredenciaisValidas_RetornaToken()
    {
        var usuario = new Usuario("admin", _hasher.Hash("Admin@123"), Perfis.Admin);
        var service = new AuthService(new FakeUsuarioRepository(usuario), _hasher, new FakeTokenService());

        var result = await service.LoginAsync(new LoginRequest("ADMIN", "Admin@123"));

        Assert.True(result.IsSuccess);
        Assert.Equal(Perfis.Admin, result.Data!.Perfil);
    }

    [Fact]
    public async Task LoginAsync_ComSenhaErrada_RetornaUnauthorized()
    {
        var usuario = new Usuario("admin", _hasher.Hash("Admin@123"), Perfis.Admin);
        var service = new AuthService(new FakeUsuarioRepository(usuario), _hasher, new FakeTokenService());

        var result = await service.LoginAsync(new LoginRequest("admin", "errada"));

        Assert.True(result.IsFailure);
        Assert.Equal(ErrorType.Unauthorized, result.Error!.Type);
    }
}
