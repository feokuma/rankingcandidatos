namespace RankingCandidatos.Api.Contracts;

public sealed record CriarCandidatoRequest(string Nome, string Cidade, string Candidatura, string? Partido);
