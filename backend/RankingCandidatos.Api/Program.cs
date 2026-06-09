using Microsoft.EntityFrameworkCore;
using RankingCandidatos.Api.Contracts;
using RankingCandidatos.Api.Domain;
using RankingCandidatos.Api.Infra.Persistence;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddOpenApi();
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));
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

await using (var scope = app.Services.CreateAsyncScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    await db.Database.EnsureCreatedAsync();
}

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseCors("frontend");

var api = app.MapGroup("/api/candidatos");

api.MapGet("/ranking", async (AppDbContext db) =>
{
    var candidatos = await db.Candidatos
        .AsNoTracking()
        .OrderByDescending(c => c.Pontuacao)
        .ThenBy(c => c.Nome)
        .ToListAsync();

    return candidatos.Select(CandidatoResponse.From);
});

api.MapGet("/{id:guid}", async (Guid id, AppDbContext db) =>
{
    var candidato = await db.Candidatos
        .AsNoTracking()
        .FirstOrDefaultAsync(c => c.Id == id);

    if (candidato is null)
    {
        return Results.NotFound();
    }

    return Results.Ok(CandidatoResponse.From(candidato));
});

api.MapPost("", async (CriarCandidatoRequest request, AppDbContext db) =>
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

    db.Candidatos.Add(candidato);
    await db.SaveChangesAsync();

    return Results.Created($"/api/candidatos/{candidato.Id}", CandidatoResponse.From(candidato));
});

api.MapPost("/{id:guid}/avaliacoes", async (Guid id, RegistrarAvaliacaoRequest request, AppDbContext db) =>
{
    var candidato = await db.Candidatos.FirstOrDefaultAsync(c => c.Id == id);
    if (candidato is null)
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

    await db.SaveChangesAsync();

    return Results.Ok(CandidatoResponse.From(candidato));
});

app.Run();
