using RankingCandidatos.Api.Domain;

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

        Assert.Equal(4, candidato.Pontuacao);
    }
}
