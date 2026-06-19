using System.Net;
using System.Net.Http.Json;
using Microsoft.Extensions.DependencyInjection;
using RankingCandidatos.Api.Contracts;
using RankingCandidatos.Api.Domain;
using RankingCandidatos.Api.Infra.Persistence;

namespace RankingCandidatos.IntegrationTests;

public sealed class CandidatosEndpointsTests : IDisposable
{
    private readonly ApiFactory _factory;
    private readonly HttpClient _client;

    public CandidatosEndpointsTests()
    {
        _factory = new ApiFactory();
        _client = _factory.CreateClient();
    }

    [Fact]
    public async Task PostCandidato_DeveCriarCandidato()
    {
        var request = new CriarCandidatoRequest(" Ana Silva ", " Recife ", " Vereadora ", " ABC ");

        var response = await _client.PostAsJsonAsync("/api/candidatos", request);

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);

        var candidato = await response.Content.ReadFromJsonAsync<CandidatoResponse>();
        Assert.NotNull(candidato);
        Assert.NotEqual(Guid.Empty, candidato.Id);
        Assert.Equal("Ana Silva", candidato.Nome);
        Assert.Equal("Recife", candidato.Cidade);
        Assert.Equal("Vereadora", candidato.Candidatura);
        Assert.Equal("ABC", candidato.Partido);
        Assert.Equal(0, candidato.Pontuacao);
    }

    [Fact]
    public async Task PostCandidato_DeveRetornarBadRequestQuandoCamposObrigatoriosNaoForemInformados()
    {
        var request = new CriarCandidatoRequest("", "Recife", "Vereadora", null);

        var response = await _client.PostAsJsonAsync("/api/candidatos", request);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task GetCandidato_DeveRetornarCandidatoPorId()
    {
        var candidato = await CriarCandidatoAsync("Ana Silva", "Recife", "Vereadora", "ABC");

        var response = await _client.GetAsync($"/api/candidatos/{candidato.Id}");

        response.EnsureSuccessStatusCode();

        var encontrado = await response.Content.ReadFromJsonAsync<CandidatoResponse>();
        Assert.NotNull(encontrado);
        Assert.Equal(candidato.Id, encontrado.Id);
        Assert.Equal("Ana Silva", encontrado.Nome);
    }

    [Fact]
    public async Task GetCandidato_DeveRetornarNotFoundQuandoCandidatoNaoExistir()
    {
        var response = await _client.GetAsync($"/api/candidatos/{Guid.NewGuid()}");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task GetRanking_DeveRetornarCandidatosOrdenadosPorPontuacaoENome()
    {
        await AdicionarCandidatoAsync("Carla", pontosPositivos: 3, pontosNegativos: 1);
        await AdicionarCandidatoAsync("Bruno", pontosPositivos: 5, pontosNegativos: 0);
        await AdicionarCandidatoAsync("Ana", pontosPositivos: 4, pontosNegativos: 2);

        var ranking = await _client.GetFromJsonAsync<CandidatoResponse[]>("/api/candidatos/ranking");

        Assert.NotNull(ranking);
        Assert.Collection(
            ranking,
            candidato => Assert.Equal("Bruno", candidato.Nome),
            candidato => Assert.Equal("Ana", candidato.Nome),
            candidato => Assert.Equal("Carla", candidato.Nome));
    }

    [Fact]
    public async Task PostAvaliacao_DeveRegistrarAvaliacaoPositiva()
    {
        var candidato = await CriarCandidatoAsync("Ana Silva", "Recife", "Vereadora", null);

        var response = await _client.PostAsJsonAsync($"/api/candidatos/{candidato.Id}/avaliacoes", new RegistrarAvaliacaoRequest("positiva"));

        response.EnsureSuccessStatusCode();

        var atualizado = await response.Content.ReadFromJsonAsync<CandidatoResponse>();
        Assert.NotNull(atualizado);
        Assert.Equal(1, atualizado.PontosPositivos);
        Assert.Equal(0, atualizado.PontosNegativos);
        Assert.Equal(1, atualizado.Pontuacao);
    }

    [Fact]
    public async Task PostAvaliacao_DeveRegistrarAvaliacaoNegativa()
    {
        var candidato = await CriarCandidatoAsync("Ana Silva", "Recife", "Vereadora", null);

        var response = await _client.PostAsJsonAsync($"/api/candidatos/{candidato.Id}/avaliacoes", new RegistrarAvaliacaoRequest("negativa"));

        response.EnsureSuccessStatusCode();

        var atualizado = await response.Content.ReadFromJsonAsync<CandidatoResponse>();
        Assert.NotNull(atualizado);
        Assert.Equal(0, atualizado.PontosPositivos);
        Assert.Equal(1, atualizado.PontosNegativos);
        Assert.Equal(-1, atualizado.Pontuacao);
    }

    [Fact]
    public async Task PostAvaliacao_DeveRetornarBadRequestQuandoTipoForInvalido()
    {
        var candidato = await CriarCandidatoAsync("Ana Silva", "Recife", "Vereadora", null);

        var response = await _client.PostAsJsonAsync($"/api/candidatos/{candidato.Id}/avaliacoes", new RegistrarAvaliacaoRequest("neutra"));

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task PostAvaliacao_DeveRetornarNotFoundQuandoCandidatoNaoExistir()
    {
        var response = await _client.PostAsJsonAsync($"/api/candidatos/{Guid.NewGuid()}/avaliacoes", new RegistrarAvaliacaoRequest("positiva"));

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    private async Task<CandidatoResponse> CriarCandidatoAsync(string nome, string cidade, string candidatura, string? partido)
    {
        var response = await _client.PostAsJsonAsync("/api/candidatos", new CriarCandidatoRequest(nome, cidade, candidatura, partido));
        response.EnsureSuccessStatusCode();

        var candidato = await response.Content.ReadFromJsonAsync<CandidatoResponse>();
        Assert.NotNull(candidato);

        return candidato;
    }

    private async Task AdicionarCandidatoAsync(string nome, int pontosPositivos, int pontosNegativos)
    {
        await using var scope = _factory.Services.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        db.Candidatos.Add(new Candidato
        {
            Id = Guid.NewGuid(),
            Nome = nome,
            Cidade = "Recife",
            Candidatura = "Vereadora",
            Partido = string.Empty,
            PontosPositivos = pontosPositivos,
            PontosNegativos = pontosNegativos
        });

        await db.SaveChangesAsync();
    }

    public void Dispose()
    {
        _client.Dispose();
        _factory.Dispose();
    }
}
