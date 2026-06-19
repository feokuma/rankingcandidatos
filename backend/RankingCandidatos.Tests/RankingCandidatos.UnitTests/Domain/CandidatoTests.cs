using RankingCandidatos.Api.Domain;
using Shouldly;

namespace RankingCandidatos.UnitTests.Domain;

public sealed class CandidatoTests
{
    [Fact]
    public void Pontuacao_DeveRetornarDiferencaEntrePontosPositivosENegativos()
    {
        var candidato = new Candidato
        {
            PontosPositivos = 7,
            PontosNegativos = 3
        };

        candidato.Pontuacao.ShouldBe(4);
    }
}
