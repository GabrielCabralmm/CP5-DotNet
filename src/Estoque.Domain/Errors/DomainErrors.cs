using Estoque.Domain.Common;

namespace Estoque.Domain.Errors;

public static class FornecedorErrors
{
    public static Error NaoEncontrado(int id) =>
        Error.NotFound("Fornecedor.NaoEncontrado", $"Fornecedor com id {id} não foi encontrado.");

    public static Error CnpjDuplicado(string cnpj) =>
        Error.Conflict("Fornecedor.CnpjDuplicado", $"Já existe um fornecedor cadastrado com o CNPJ {cnpj}.");

    public static Error PossuiProdutos(int id) =>
        Error.Conflict("Fornecedor.PossuiProdutos", $"O fornecedor {id} possui produtos vinculados e não pode ser removido.");

    public static Error FalhaAoSalvar(string detalhe) =>
        Error.Unexpected("Fornecedor.FalhaAoSalvar", $"Não foi possível salvar o fornecedor: {detalhe}");
}

public static class ProdutoErrors
{
    public static Error NaoEncontrado(int id) =>
        Error.NotFound("Produto.NaoEncontrado", $"Produto com id {id} não foi encontrado.");

    public static Error SkuDuplicado(string sku) =>
        Error.Conflict("Produto.SkuDuplicado", $"Já existe um produto cadastrado com o SKU {sku}.");

    public static Error FornecedorInexistente(int fornecedorId) =>
        Error.Validation("Produto.FornecedorInexistente", $"O fornecedor {fornecedorId} informado para o produto não existe.");
}

public static class CepErrors
{
    public static Error Invalido(string? cep) =>
        Error.Validation("Cep.Invalido", $"O CEP '{cep}' é inválido. Informe 8 dígitos.");

    public static Error NaoEncontrado(string cep) =>
        Error.NotFound("Cep.NaoEncontrado", $"O CEP {cep} não foi encontrado no serviço ViaCEP.");

    public static Error ServicoIndisponivel(string detalhe) =>
        Error.ExternalService("Cep.ServicoIndisponivel", $"O serviço externo ViaCEP está indisponível no momento ({detalhe}). Tente novamente.");
}

public static class AuthErrors
{
    public static Error CredenciaisInvalidas() =>
        Error.Unauthorized("Auth.CredenciaisInvalidas", "Usuário ou senha inválidos.");

    public static Error NaoAutenticado() =>
        Error.Unauthorized("Auth.NaoAutenticado", "Token ausente, inválido ou expirado. Faça login em /api/v1/auth/login.");

    public static Error AcessoNegado() =>
        Error.Forbidden("Auth.AcessoNegado", "Seu perfil não tem permissão para executar esta operação.");
}
