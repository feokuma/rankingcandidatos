using Microsoft.EntityFrameworkCore;
using RankingCandidatos.Api.Contracts;
using RankingCandidatos.Api.Domain;
using RankingCandidatos.Api.Infra.Persistence;
using RankingCandidatos.Api.Services;

namespace RankingCandidatos.Api.Endpoints;

public static class CandidatosEndpoints
{
    public static RouteGroupBuilder MapCandidatosEndpoints(this IEndpointRouteBuilder app)
    {
        var api = app.MapGroup("/api/candidatos");

        api.MapGet("/ranking", async (AppDbContext db) =>
        {
            var candidatos = await db.Candidatos
                .AsNoTracking()
                .OrderByDescending(c => c.PontosPositivos - c.PontosNegativos)
                .ThenBy(c => c.Nome)
                .ToListAsync();

            return candidatos.Select(CandidatoResponse.From);
        });

        api.MapGet("/externos/presidencia/2022", async Task<IResult> (CandidatosExternosService service) =>
        {
            try
            {
                var candidatos = await service.ObterPresidenciais2022Async();
                return Results.Ok(candidatos);
            }
            catch (HttpRequestException)
            {
                return Results.Json(new { erro = "Nao foi possivel consultar os candidatos externos no momento." }, statusCode: StatusCodes.Status502BadGateway);
            }
            catch (TaskCanceledException)
            {
                return Results.Json(new { erro = "Nao foi possivel consultar os candidatos externos no momento." }, statusCode: StatusCodes.Status502BadGateway);
            }
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

        return api;
    }
}
