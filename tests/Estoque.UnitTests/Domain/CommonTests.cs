using Estoque.Domain.Common;
using Estoque.Domain.Errors;

namespace Estoque.UnitTests.Domain;

public sealed class OperationResultTests
{
    [Fact]
    public void Success_ContemDadosESemErro()
    {
        var result = OperationResult<int>.Success(42);

        Assert.True(result.IsSuccess);
        Assert.False(result.IsFailure);
        Assert.Equal(42, result.Data);
        Assert.Null(result.Error);
    }

    [Fact]
    public void ConversaoImplicitaDeErro_GeraFalha()
    {
        OperationResult<string> result = ProdutoErrors.NaoEncontrado(7);

        Assert.True(result.IsFailure);
        Assert.Equal("Produto.NaoEncontrado", result.Error!.Code);
        Assert.Equal(ErrorType.NotFound, result.Error.Type);
    }

    [Fact]
    public void Map_PreservaErroQuandoFalha()
    {
        OperationResult<int> result = CepErrors.Invalido("123");

        var mapeado = result.Map(x => x.ToString());

        Assert.True(mapeado.IsFailure);
        Assert.Equal("Cep.Invalido", mapeado.Error!.Code);
    }
}

public sealed class PageResultTests
{
    [Theory]
    [InlineData(1, 10, 25, 3, false, true)]
    [InlineData(2, 10, 25, 3, true, true)]
    [InlineData(3, 10, 25, 3, true, false)]
    [InlineData(1, 10, 0, 0, false, false)]
    public void CalculaMetadadosDePaginacao(int page, int size, int total, int totalPages, bool hasPrevious, bool hasNext)
    {
        var result = new PageResult<int>(Array.Empty<int>(), page, size, total);

        Assert.Equal(totalPages, result.TotalPages);
        Assert.Equal(hasPrevious, result.HasPrevious);
        Assert.Equal(hasNext, result.HasNext);
    }

    [Theory]
    [InlineData(0, 0, 1, 10)]
    [InlineData(-3, 500, 1, 50)]
    [InlineData(4, 20, 4, 20)]
    public void PageRequest_NormalizaValores(int page, int size, int expectedPage, int expectedSize)
    {
        var request = PageRequest.Create(page, size);

        Assert.Equal(expectedPage, request.PageNumber);
        Assert.Equal(expectedSize, request.PageSize);
        Assert.Equal((expectedPage - 1) * expectedSize, request.Skip);
    }
}
