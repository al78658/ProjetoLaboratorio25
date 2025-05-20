using Microsoft.EntityFrameworkCore;
using ProjetoLaboratorio25.Models;

namespace ProjetoLaboratorio25.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<Utilizador> Utilizadores { get; set; }
        public DbSet<Competicao> Competicoes { get; set; }
        public DbSet<ConfiguracaoFase> ConfiguracoesFase { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configure Utilizador
            modelBuilder.Entity<Utilizador>()
                .HasKey(u => u.Id);

            modelBuilder.Entity<Utilizador>()
                .Property(u => u.UtilizadorNome)
                .IsRequired();

            modelBuilder.Entity<Utilizador>()
                .Property(u => u.Senha)
                .IsRequired();

            // Configure Competicao
            modelBuilder.Entity<Competicao>()
                .HasKey(c => c.Id);

            modelBuilder.Entity<Competicao>()
                .Property(c => c.Nome)
                .IsRequired();

            modelBuilder.Entity<Competicao>()
                .Property(c => c.TipoCompeticao)
                .IsRequired();

            // Configure ConfiguracaoFase
            modelBuilder.Entity<ConfiguracaoFase>()
                .HasKey(cf => cf.Id);

            // Configure CriteriosDesempate como JSON
            modelBuilder.Entity<ConfiguracaoFase>()
                .Property(cf => cf.CriteriosDesempate)
                .HasConversion(
                    v => System.Text.Json.JsonSerializer.Serialize(v, new System.Text.Json.JsonSerializerOptions()),
                    v => System.Text.Json.JsonSerializer.Deserialize<List<string>>(v, new System.Text.Json.JsonSerializerOptions()) ?? new List<string>()
                );

            // Configure the relationship between Competicao and ConfiguracaoFase
            modelBuilder.Entity<ConfiguracaoFase>()
                .HasOne(cf => cf.Competicao)
                .WithMany(c => c.ConfiguracoesFase)
                .HasForeignKey(cf => cf.CompeticaoId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
} 