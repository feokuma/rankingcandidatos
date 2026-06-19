using RankingCandidatos.Api.Contracts;
using RankingCandidatos.Api.Domain;

namespace RankingCandidatos.UnitTests.Contracts;

public sealed class CandidatoResponseTests
{
    [Fact]
    public void From_DeveMapearCandidatoParaResponse()
    {
        var candidato = new Candidato
        {
            Id = Guid.NewGuid(),
            Nome = "Ana Silva",
            Cidade = "Recife",
            Partido = "ABC",
            Candidatura = "Vereadora",
            PontosPositivos = 5,
            PontosNegativos = 2
        };

        var response = CandidatoResponse.From(candidato);

        Assert.Equal(candidato.Id, response.Id);
        Assert.Equal("Ana Silva", response.Nome);
        Assert.Equal("Recife", response.Cidade);
        Assert.Equal("ABC", response.Partido);
        Assert.Equal("Vereadora", response.Candidatura);
        Assert.Equal(5, response.PontosPositivos);
        Assert.Equal(2, response.PontosNegativos);
        Assert.Equal(3, response.Pontuacao);
    }
}
