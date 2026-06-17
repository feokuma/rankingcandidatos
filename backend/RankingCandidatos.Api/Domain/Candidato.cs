namespace RankingCandidatos.Api.Domain;

public sealed class Candidato
{
    public Guid Id { get; set; }
    public string Nome { get; set; } = string.Empty;
    public string Cidade { get; set; } = string.Empty;
    public string Partido { get; set; } = string.Empty;
    public string Candidatura { get; set; } = string.Empty;
    public int PontosPositivos { get; set; }
    public int PontosNegativos { get; set; }
    public int Pontuacao => PontosPositivos - PontosNegativos;
}
