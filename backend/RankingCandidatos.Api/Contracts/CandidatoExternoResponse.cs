namespace RankingCandidatos.Api.Contracts;

public sealed record CandidatoExternoResponse(
    long IdExterno,
    string Nome,
    string Partido,
    int Numero,
    string? FotoUrl
);
