using System.Collections.Concurrent;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddOpenApi();
builder.Services.AddCors(options =>
{
    options.AddPolicy("frontend", policy =>
    {
        policy.WithOrigins("http://localhost:3000")
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseCors("frontend");

var candidatos = new ConcurrentDictionary<Guid, Candidato>();

var api = app.MapGroup("/api/candidatos");

api.MapGet("/ranking", () =>
{
    return candidatos.Values
        .OrderByDescending(c => c.Pontuacao)
        .ThenBy(c => c.Nome)
        .Select(CandidatoResponse.From)
        .ToArray();
});

api.MapGet("/{id:guid}", (Guid id) =>
{
    if (!candidatos.TryGetValue(id, out var candidato))
    {
        return Results.NotFound();
    }

    return Results.Ok(CandidatoResponse.From(candidato));
});

api.MapPost("", (CriarCandidatoRequest request) =>
{
    if (string.IsNullOrWhiteSpace(request.Nome) || string.IsNullOrWhiteSpace(request.Cidade) || string.IsNullOrWhiteSpace(request.Candidatura))
    {
        return Results.BadRequest(new { erro = "Nome, cidade e candidatura são obrigatórios." });
    }

    var candidato = new Candidato
    {
        Id = Guid.NewGuid(),
        Nome = request.Nome.Trim(),
        Cidade = request.Cidade.Trim(),
        Candidatura = request.Candidatura.Trim(),
        Partido = request.Partido?.Trim() ?? string.Empty
    };

    candidatos[candidato.Id] = candidato;

    return Results.Created($"/api/candidatos/{candidato.Id}", CandidatoResponse.From(candidato));
});

api.MapPost("/{id:guid}/avaliacoes", (Guid id, RegistrarAvaliacaoRequest request) =>
{
    if (!candidatos.TryGetValue(id, out var candidato))
    {
        return Results.NotFound();
    }

    var tipo = request.Tipo.Trim().ToLowerInvariant();
    switch (tipo)
    {
        case "positiva":
            candidato.PontosPositivos++;
            break;
        case "negativa":
            candidato.PontosNegativos++;
            break;
        default:
            return Results.BadRequest(new { erro = "Tipo de avaliação inválido. Use 'positiva' ou 'negativa'." });
    }

    return Results.Ok(CandidatoResponse.From(candidato));
});

app.Run();

sealed class Candidato
{
    public Guid Id { get; init; }
    public string Nome { get; init; } = string.Empty;
    public string Cidade { get; init; } = string.Empty;
    public string Partido { get; init; } = string.Empty;
    public string Candidatura { get; init; } = string.Empty;
    public int PontosPositivos { get; set; }
    public int PontosNegativos { get; set; }
    public int Pontuacao => PontosPositivos - PontosNegativos;
}

record CriarCandidatoRequest(string Nome, string Cidade, string Candidatura, string? Partido);
record RegistrarAvaliacaoRequest(string Tipo);

record CandidatoResponse(
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
