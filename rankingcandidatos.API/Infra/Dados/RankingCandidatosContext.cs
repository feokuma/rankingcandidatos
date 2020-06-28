using Microsoft.EntityFrameworkCore;
using rankingcandidatos.API.Dominio;

namespace rankingcandidatos.API.Infra.Dados
{
    public class RankingCandidatosContext : DbContext
    {
        public RankingCandidatosContext(DbContextOptions<RankingCandidatosContext> options) : base(options) { }

        public DbSet<Candidato> Candidatos { get; set; }
    }
}
