namespace RankingCandidatos.Api.Services;

public sealed class CandidatosExternosOptions
{
    public const string SectionName = "CandidatosExternos";

    public string BaseUrl { get; set; } = "https://divulgacandcontas.tse.jus.br";

    public string Presidenciais2022Path { get; set; } = "/divulga/rest/v1/candidatura/listar/2022/BR/2040602022/1/candidatos";
}
