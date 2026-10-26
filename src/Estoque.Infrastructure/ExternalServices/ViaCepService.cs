using System.Net;
using System.Text.Json;
using Estoque.Application.DTOs;
using Estoque.Application.Interfaces;
using Estoque.Domain.Common;
using Estoque.Domain.Errors;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;

namespace Estoque.Infrastructure.ExternalServices;

public sealed class ViaCepService(HttpClient httpClient, IMemoryCache cache, ILogger<ViaCepService> logger) : ICepService
{
    private static readonly TimeSpan CacheDuration = TimeSpan.FromHours(6);

    public async Task<OperationResult<EnderecoCepResponse>> ConsultarAsync(string cep, CancellationToken ct = default)
    {
        var digits = Guard.OnlyDigits(cep);
        if (digits.Length != 8)
            return CepErrors.Invalido(cep);

        var cacheKey = $"viacep:{digits}";
        if (cache.TryGetValue(cacheKey, out EnderecoCepResponse? cached) && cached is not null)
        {
            logger.LogInformation("CEP {Cep} obtido do cache.", digits);
            return OperationResult<EnderecoCepResponse>.Success(cached);
        }

        try
        {
            using var response = await httpClient.GetAsync($"{digits}/json/", ct);

            if (response.StatusCode == HttpStatusCode.BadRequest)
                return CepErrors.Invalido(cep);

            if (!response.IsSuccessStatusCode)
                return CepErrors.ServicoIndisponivel($"HTTP {(int)response.StatusCode}");

            await using var stream = await response.Content.ReadAsStreamAsync(ct);
            using var document = await JsonDocument.ParseAsync(stream, cancellationToken: ct);
            var root = document.RootElement;

            if (root.ValueKind != JsonValueKind.Object || root.TryGetProperty("erro", out _))
                return CepErrors.NaoEncontrado(digits);

            var endereco = new EnderecoCepResponse(
                digits,
                Texto(root, "logradouro"),
                Texto(root, "bairro"),
                Texto(root, "localidade") ?? string.Empty,
                Texto(root, "uf") ?? string.Empty);

            cache.Set(cacheKey, endereco, CacheDuration);
            logger.LogInformation("CEP {Cep} consultado no ViaCEP com sucesso.", digits);
            return OperationResult<EnderecoCepResponse>.Success(endereco);
        }
        catch (JsonException ex)
        {
            logger.LogWarning(ex, "Resposta inválida do ViaCEP para o CEP {Cep}.", digits);
            return CepErrors.ServicoIndisponivel("resposta inválida");
        }
        catch (Exception ex) when (ex is not OperationCanceledException || !ct.IsCancellationRequested)
        {
            logger.LogWarning(ex, "Falha ao consultar o ViaCEP para o CEP {Cep}.", digits);
            return CepErrors.ServicoIndisponivel(ex.GetType().Name);
        }
    }

    private static string? Texto(JsonElement root, string propriedade) =>
        root.TryGetProperty(propriedade, out var valor) && valor.ValueKind == JsonValueKind.String && !string.IsNullOrWhiteSpace(valor.GetString())
            ? valor.GetString()
            : null;
}
