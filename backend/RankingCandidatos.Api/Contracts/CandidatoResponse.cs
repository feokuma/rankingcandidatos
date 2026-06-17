using RankingCandidatos.Api.Domain;

namespace RankingCandidatos.Api.Contracts;

public sealed record CandidatoResponse(
    Guid Id,
    string Nome,
    string Cidade,
    string Partido,
    string Candidatura,
    int PontosPositivos,
    int PontosNegativos,
    int Pontuacao)
{
    public static CandidatoResponse From(Candidato candidato)
    {
        return new CandidatoResponse(
            candidato.Id,
            candidato.Nome,
            candidato.Cidade,
            candidato.Partido,
            candidato.Candidatura,
            candidato.PontosPositivos,
            candidato.PontosNegativos,
            candidato.Pontuacao);
    }
}
