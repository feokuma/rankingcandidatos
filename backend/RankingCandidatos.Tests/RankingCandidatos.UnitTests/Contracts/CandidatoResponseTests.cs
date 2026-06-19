using RankingCandidatos.Api.Contracts;
using RankingCandidatos.Api.Domain;
using Shouldly;

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

        response.Id.ShouldBe(candidato.Id);
        response.Nome.ShouldBe("Ana Silva");
        response.Cidade.ShouldBe("Recife");
        response.Partido.ShouldBe("ABC");
        response.Candidatura.ShouldBe("Vereadora");
        response.PontosPositivos.ShouldBe(5);
        response.PontosNegativos.ShouldBe(2);
        response.Pontuacao.ShouldBe(3);
    }
}
