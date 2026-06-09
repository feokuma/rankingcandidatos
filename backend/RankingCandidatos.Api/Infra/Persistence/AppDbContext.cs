using Microsoft.EntityFrameworkCore;
using RankingCandidatos.Api.Domain;

namespace RankingCandidatos.Api.Infra.Persistence;

public sealed class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<Candidato> Candidatos => Set<Candidato>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Candidato>(entity =>
        {
            entity.ToTable("candidatos");
            entity.HasKey(c => c.Id);
            entity.Property(c => c.Nome).IsRequired().HasMaxLength(150);
            entity.Property(c => c.Cidade).IsRequired().HasMaxLength(120);
            entity.Property(c => c.Partido).HasMaxLength(50);
            entity.Property(c => c.Candidatura).IsRequired().HasMaxLength(80);
        });
    }
}
