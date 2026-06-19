using System.Net;
using System.Net.Http.Json;
using Microsoft.Extensions.DependencyInjection;
using RankingCandidatos.Api.Contracts;
using RankingCandidatos.Api.Domain;
using RankingCandidatos.Api.Infra.Persistence;
using Shouldly;

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

        response.StatusCode.ShouldBe(HttpStatusCode.Created);

        var candidato = await response.Content.ReadFromJsonAsync<CandidatoResponse>();
        candidato.ShouldNotBeNull();
        candidato.Id.ShouldNotBe(Guid.Empty);
        candidato.Nome.ShouldBe("Ana Silva");
        candidato.Cidade.ShouldBe("Recife");
        candidato.Candidatura.ShouldBe("Vereadora");
        candidato.Partido.ShouldBe("ABC");
        candidato.Pontuacao.ShouldBe(0);
    }

    [Fact]
    public async Task PostCandidato_DeveRetornarBadRequestQuandoCamposObrigatoriosNaoForemInformados()
    {
        var request = new CriarCandidatoRequest("", "Recife", "Vereadora", null);

        var response = await _client.PostAsJsonAsync("/api/candidatos", request);

        response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task GetCandidato_DeveRetornarCandidatoPorId()
    {
        var candidato = await CriarCandidatoAsync("Ana Silva", "Recife", "Vereadora", "ABC");

        var response = await _client.GetAsync($"/api/candidatos/{candidato.Id}");

        response.EnsureSuccessStatusCode();

        var encontrado = await response.Content.ReadFromJsonAsync<CandidatoResponse>();
        encontrado.ShouldNotBeNull();
        encontrado.Id.ShouldBe(candidato.Id);
        encontrado.Nome.ShouldBe("Ana Silva");
    }

    [Fact]
    public async Task GetCandidato_DeveRetornarNotFoundQuandoCandidatoNaoExistir()
    {
        var response = await _client.GetAsync($"/api/candidatos/{Guid.NewGuid()}");

        response.StatusCode.ShouldBe(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task GetRanking_DeveRetornarCandidatosOrdenadosPorPontuacaoENome()
    {
        await AdicionarCandidatoAsync("Carla", pontosPositivos: 3, pontosNegativos: 1);
        await AdicionarCandidatoAsync("Bruno", pontosPositivos: 5, pontosNegativos: 0);
        await AdicionarCandidatoAsync("Ana", pontosPositivos: 4, pontosNegativos: 2);

        var ranking = await _client.GetFromJsonAsync<CandidatoResponse[]>("/api/candidatos/ranking");

        ranking.ShouldNotBeNull();
        ranking.Select(candidato => candidato.Nome).ShouldBe(["Bruno", "Ana", "Carla"]);
    }

    [Fact]
    public async Task PostAvaliacao_DeveRegistrarAvaliacaoPositiva()
    {
        var candidato = await CriarCandidatoAsync("Ana Silva", "Recife", "Vereadora", null);

        var response = await _client.PostAsJsonAsync($"/api/candidatos/{candidato.Id}/avaliacoes", new RegistrarAvaliacaoRequest("positiva"));

        response.EnsureSuccessStatusCode();

        var atualizado = await response.Content.ReadFromJsonAsync<CandidatoResponse>();
        atualizado.ShouldNotBeNull();
        atualizado.PontosPositivos.ShouldBe(1);
        atualizado.PontosNegativos.ShouldBe(0);
        atualizado.Pontuacao.ShouldBe(1);
    }

    [Fact]
    public async Task PostAvaliacao_DeveRegistrarAvaliacaoNegativa()
    {
        var candidato = await CriarCandidatoAsync("Ana Silva", "Recife", "Vereadora", null);

        var response = await _client.PostAsJsonAsync($"/api/candidatos/{candidato.Id}/avaliacoes", new RegistrarAvaliacaoRequest("negativa"));

        response.EnsureSuccessStatusCode();

        var atualizado = await response.Content.ReadFromJsonAsync<CandidatoResponse>();
        atualizado.ShouldNotBeNull();
        atualizado.PontosPositivos.ShouldBe(0);
        atualizado.PontosNegativos.ShouldBe(1);
        atualizado.Pontuacao.ShouldBe(-1);
    }

    [Fact]
    public async Task PostAvaliacao_DeveRetornarBadRequestQuandoTipoForInvalido()
    {
        var candidato = await CriarCandidatoAsync("Ana Silva", "Recife", "Vereadora", null);

        var response = await _client.PostAsJsonAsync($"/api/candidatos/{candidato.Id}/avaliacoes", new RegistrarAvaliacaoRequest("neutra"));

        response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task PostAvaliacao_DeveRetornarNotFoundQuandoCandidatoNaoExistir()
    {
        var response = await _client.PostAsJsonAsync($"/api/candidatos/{Guid.NewGuid()}/avaliacoes", new RegistrarAvaliacaoRequest("positiva"));

        response.StatusCode.ShouldBe(HttpStatusCode.NotFound);
    }

    private async Task<CandidatoResponse> CriarCandidatoAsync(string nome, string cidade, string candidatura, string? partido)
    {
        var response = await _client.PostAsJsonAsync("/api/candidatos", new CriarCandidatoRequest(nome, cidade, candidatura, partido));
        response.EnsureSuccessStatusCode();

        var candidato = await response.Content.ReadFromJsonAsync<CandidatoResponse>();
        candidato.ShouldNotBeNull();

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
