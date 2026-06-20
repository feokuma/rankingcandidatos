using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using RankingCandidatos.Api.Contracts;
using RankingCandidatos.Api.Domain;
using RankingCandidatos.Api.Infra.Persistence;
using RankingCandidatos.Api.Services;
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

        [Fact]
        public async Task GetCandidatosExternosPresidencia2022_DeveRetornarListaMapeada()
        {
                const string payload = """
                {
                    "candidatos": [
                        {
                            "id": 1001,
                            "nomeUrna": "CANDIDATO A",
                            "numero": 10,
                            "fotoUrl": "https://example.org/a.jpg",
                            "partido": { "sigla": "AAA" }
                        },
                        {
                            "id": 1002,
                            "nomeUrna": "CANDIDATO B",
                            "numero": 22,
                            "fotoUrl": null,
                            "partido": { "sigla": "BBB" }
                        }
                    ]
                }
                """;

                using var client = CreateClientComRespostaExterna(new HttpResponseMessage(HttpStatusCode.OK)
                {
                        Content = new StringContent(payload, Encoding.UTF8, "application/json")
                });

                var response = await client.GetAsync("/api/candidatos/externos/presidencia/2022");

                response.EnsureSuccessStatusCode();

                var candidatos = await response.Content.ReadFromJsonAsync<CandidatoExternoResponse[]>();
                candidatos.ShouldNotBeNull();
                candidatos.Length.ShouldBe(2);
                candidatos[0].IdExterno.ShouldBe(1001);
                candidatos[0].Nome.ShouldBe("CANDIDATO A");
                candidatos[0].Partido.ShouldBe("AAA");
                candidatos[0].Numero.ShouldBe(10);
                candidatos[0].FotoUrl.ShouldBe("https://example.org/a.jpg");
        }

        [Fact]
        public async Task GetCandidatosExternosPresidencia2022_DeveRetornarBadGatewayQuandoProvedorFalhar()
        {
                using var client = CreateClientComRespostaExterna(new HttpResponseMessage(HttpStatusCode.ServiceUnavailable));

                var response = await client.GetAsync("/api/candidatos/externos/presidencia/2022");

                response.StatusCode.ShouldBe(HttpStatusCode.BadGateway);

                var erro = await response.Content.ReadFromJsonAsync<Dictionary<string, string>>();
                erro.ShouldNotBeNull();
                erro.ShouldContainKey("erro");
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

    private HttpClient CreateClientComRespostaExterna(HttpResponseMessage response)
    {
        return _factory.WithWebHostBuilder(builder =>
        {
            builder.ConfigureServices(services =>
            {
                services.RemoveAll<CandidatosExternosService>();
                services.AddSingleton(_ =>
                {
                    var httpClient = new HttpClient(new StubHttpMessageHandler(_ => response))
                    {
                        BaseAddress = new Uri("https://divulgacandcontas.tse.jus.br")
                    };

                    var options = Microsoft.Extensions.Options.Options.Create(new CandidatosExternosOptions
                    {
                        BaseUrl = "https://divulgacandcontas.tse.jus.br",
                        Presidenciais2022Path = "/divulga/rest/v1/candidatura/listar/2022/BR/2040602022/1/candidatos"
                    });

                    return new CandidatosExternosService(httpClient, options);
                });
            });
        }).CreateClient();
    }

    private sealed class StubHttpMessageHandler : HttpMessageHandler
    {
        private readonly Func<HttpRequestMessage, HttpResponseMessage> _handler;

        public StubHttpMessageHandler(Func<HttpRequestMessage, HttpResponseMessage> handler)
        {
            _handler = handler;
        }

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            return Task.FromResult(_handler(request));
        }
    }
}
