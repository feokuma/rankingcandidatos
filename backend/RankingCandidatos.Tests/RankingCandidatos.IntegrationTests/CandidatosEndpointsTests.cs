using System.Net;
using System.Net.Http.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using RankingCandidatos.Api.Contracts;
using RankingCandidatos.Api.Domain;
using RankingCandidatos.Api.Infra.Persistence;
using Shouldly;

namespace RankingCandidatos.IntegrationTests;

public sealed class CandidatosEndpointsTests : IClassFixture<ApiFactory>, IAsyncLifetime
{
    private readonly ApiFactory _factory;
    private readonly HttpClient _client;

    public CandidatosEndpointsTests(ApiFactory factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
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

        var candidatoNoBanco = await ObterCandidatoNoBancoAsync(candidato.Id);
        candidatoNoBanco.ShouldNotBeNull();
        candidatoNoBanco.Nome.ShouldBe("Ana Silva");
        candidatoNoBanco.Cidade.ShouldBe("Recife");
        candidatoNoBanco.Candidatura.ShouldBe("Vereadora");
        candidatoNoBanco.Partido.ShouldBe("ABC");
    }

    [Fact]
    public async Task PostCandidato_DeveRetornarBadRequestQuandoCamposObrigatoriosNaoForemInformados()
    {
        var request = new CriarCandidatoRequest("", "Recife", "Vereadora", null);

        var response = await _client.PostAsJsonAsync("/api/candidatos", request);

        response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);

        var candidatosNoBanco = await ListarCandidatosNoBancoAsync();
        candidatosNoBanco.ShouldBeEmpty();
    }

    [Fact]
    public async Task GetCandidato_DeveRetornarCandidatoPorId()
    {
        var candidato = await AdicionarCandidatoAsync("Ana Silva", "Recife", "Vereadora", "ABC");

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
        var candidato = await AdicionarCandidatoAsync("Ana Silva");

        var response = await _client.PostAsJsonAsync($"/api/candidatos/{candidato.Id}/avaliacoes", new RegistrarAvaliacaoRequest("positiva"));

        response.EnsureSuccessStatusCode();

        var atualizado = await response.Content.ReadFromJsonAsync<CandidatoResponse>();
        atualizado.ShouldNotBeNull();
        atualizado.PontosPositivos.ShouldBe(1);
        atualizado.PontosNegativos.ShouldBe(0);
        atualizado.Pontuacao.ShouldBe(1);

        var candidatoNoBanco = await ObterCandidatoNoBancoAsync(candidato.Id);
        candidatoNoBanco.ShouldNotBeNull();
        candidatoNoBanco.PontosPositivos.ShouldBe(1);
        candidatoNoBanco.PontosNegativos.ShouldBe(0);
    }

    [Fact]
    public async Task PostAvaliacao_DeveRegistrarAvaliacaoNegativa()
    {
        var candidato = await AdicionarCandidatoAsync("Ana Silva");

        var response = await _client.PostAsJsonAsync($"/api/candidatos/{candidato.Id}/avaliacoes", new RegistrarAvaliacaoRequest("negativa"));

        response.EnsureSuccessStatusCode();

        var atualizado = await response.Content.ReadFromJsonAsync<CandidatoResponse>();
        atualizado.ShouldNotBeNull();
        atualizado.PontosPositivos.ShouldBe(0);
        atualizado.PontosNegativos.ShouldBe(1);
        atualizado.Pontuacao.ShouldBe(-1);

        var candidatoNoBanco = await ObterCandidatoNoBancoAsync(candidato.Id);
        candidatoNoBanco.ShouldNotBeNull();
        candidatoNoBanco.PontosPositivos.ShouldBe(0);
        candidatoNoBanco.PontosNegativos.ShouldBe(1);
    }

    [Fact]
    public async Task PostAvaliacao_DeveRetornarBadRequestQuandoTipoForInvalido()
    {
        var candidato = await AdicionarCandidatoAsync("Ana Silva");

        var response = await _client.PostAsJsonAsync($"/api/candidatos/{candidato.Id}/avaliacoes", new RegistrarAvaliacaoRequest("neutra"));

        response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);

        var candidatoNoBanco = await ObterCandidatoNoBancoAsync(candidato.Id);
        candidatoNoBanco.ShouldNotBeNull();
        candidatoNoBanco.PontosPositivos.ShouldBe(0);
        candidatoNoBanco.PontosNegativos.ShouldBe(0);
    }

    [Fact]
    public async Task PostAvaliacao_DeveRetornarNotFoundQuandoCandidatoNaoExistir()
    {
        var response = await _client.PostAsJsonAsync($"/api/candidatos/{Guid.NewGuid()}/avaliacoes", new RegistrarAvaliacaoRequest("positiva"));

        response.StatusCode.ShouldBe(HttpStatusCode.NotFound);
    }

    private async Task<Candidato> AdicionarCandidatoAsync(
        string nome,
        string cidade = "Recife",
        string candidatura = "Vereadora",
        string? partido = null,
        int pontosPositivos = 0,
        int pontosNegativos = 0)
    {
        await using var scope = _factory.Services.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        var candidato = new Candidato
        {
            Id = Guid.NewGuid(),
            Nome = nome,
            Cidade = cidade,
            Candidatura = candidatura,
            Partido = partido ?? string.Empty,
            PontosPositivos = pontosPositivos,
            PontosNegativos = pontosNegativos
        };

        db.Candidatos.Add(candidato);

        await db.SaveChangesAsync();

        return candidato;
    }

    private async Task<Candidato?> ObterCandidatoNoBancoAsync(Guid id)
    {
        await using var scope = _factory.Services.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        return await db.Candidatos
            .AsNoTracking()
            .SingleOrDefaultAsync(candidato => candidato.Id == id);
    }

    private async Task<Candidato[]> ListarCandidatosNoBancoAsync()
    {
        await using var scope = _factory.Services.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        return await db.Candidatos
            .AsNoTracking()
            .ToArrayAsync();
    }

    public async Task InitializeAsync()
    {
        await LimparBancoAsync();
    }

    public Task DisposeAsync()
    {
        _client.Dispose();

        return Task.CompletedTask;
    }

    private async Task LimparBancoAsync()
    {
        await using var scope = _factory.Services.CreateAsyncScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        db.Candidatos.RemoveRange(db.Candidatos);
        await db.SaveChangesAsync();
    }
}
