using System.Net.Http.Json;
using System.Text.Json.Serialization;
using Microsoft.Extensions.Options;
using RankingCandidatos.Api.Contracts;

namespace RankingCandidatos.Api.Services;

public sealed class CandidatosExternosService
{
    private readonly HttpClient _httpClient;
    private readonly CandidatosExternosOptions _options;

    public CandidatosExternosService(HttpClient httpClient, IOptions<CandidatosExternosOptions> options)
    {
        _httpClient = httpClient;
        _options = options.Value;
    }

    public async Task<IReadOnlyList<CandidatoExternoResponse>> ObterPresidenciais2022Async(CancellationToken cancellationToken = default)
    {
        var response = await _httpClient.GetAsync(_options.Presidenciais2022Path, cancellationToken);
        response.EnsureSuccessStatusCode();

        var payload = await response.Content.ReadFromJsonAsync<TseListaCandidatosResponse>(cancellationToken: cancellationToken);
        var candidatos = payload?.Candidatos ?? [];

        return candidatos
            .Where(candidato => !string.IsNullOrWhiteSpace(candidato.NomeUrna))
            .Select(candidato => new CandidatoExternoResponse(
                candidato.Id,
                candidato.NomeUrna!.Trim(),
                candidato.Partido?.Sigla?.Trim() ?? string.Empty,
                candidato.Numero,
                candidato.FotoUrl))
            .ToArray();
    }

    private sealed class TseListaCandidatosResponse
    {
        [JsonPropertyName("candidatos")]
        public List<TseCandidato> Candidatos { get; init; } = [];
    }

    private sealed class TseCandidato
    {
        [JsonPropertyName("id")]
        public long Id { get; init; }

        [JsonPropertyName("nomeUrna")]
        public string? NomeUrna { get; init; }

        [JsonPropertyName("numero")]
        public int Numero { get; init; }

        [JsonPropertyName("fotoUrl")]
        public string? FotoUrl { get; init; }

        [JsonPropertyName("partido")]
        public TsePartido? Partido { get; init; }
    }

    private sealed class TsePartido
    {
        [JsonPropertyName("sigla")]
        public string? Sigla { get; init; }
    }
}
