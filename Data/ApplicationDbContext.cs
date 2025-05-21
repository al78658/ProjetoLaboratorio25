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
        public DbSet<Jogador> Jogadores { get; set; }
        public DbSet<JogoEmparelhado> JogosEmparelhados { get; set; }
        public DbSet<EmparelhamentoBase> EmparelhamentosBase { get; set; } // o nome correto

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Utilizador
            modelBuilder.Entity<Utilizador>()
                .HasKey(u => u.Id);
            modelBuilder.Entity<Utilizador>()
                .Property(u => u.UtilizadorNome).IsRequired();
            modelBuilder.Entity<Utilizador>()
                .Property(u => u.Senha).IsRequired();

            // Competicao
            modelBuilder.Entity<Competicao>()
                .HasKey(c => c.Id);
            modelBuilder.Entity<Competicao>()
                .Property(c => c.Nome).IsRequired();
            modelBuilder.Entity<Competicao>()
                .Property(c => c.TipoCompeticao).IsRequired();

            // ConfiguracaoFase
            modelBuilder.Entity<ConfiguracaoFase>()
                .HasKey(cf => cf.Id);
            modelBuilder.Entity<ConfiguracaoFase>()
                .Property(cf => cf.CriteriosDesempate).HasConversion(
                    v => System.Text.Json.JsonSerializer.Serialize(v, new System.Text.Json.JsonSerializerOptions()),
                    v => System.Text.Json.JsonSerializer.Deserialize<List<string>>(v, new System.Text.Json.JsonSerializerOptions()) ?? new List<string>()
                );
            modelBuilder.Entity<ConfiguracaoFase>()
                .HasOne(cf => cf.Competicao)
                .WithMany(c => c.ConfiguracoesFase)
                .HasForeignKey(cf => cf.CompeticaoId)
                .OnDelete(DeleteBehavior.Cascade);

            // Jogador
            modelBuilder.Entity<Jogador>()
                .HasKey(j => j.Id);
            modelBuilder.Entity<Jogador>().Property(j => j.Nome).IsRequired();
            modelBuilder.Entity<Jogador>().Property(j => j.Codigo).IsRequired();
            modelBuilder.Entity<Jogador>().Property(j => j.DataNascimento).IsRequired();
            modelBuilder.Entity<Jogador>().Property(j => j.Categoria).IsRequired();
            modelBuilder.Entity<Jogador>().Property(j => j.Clube).IsRequired();
            modelBuilder.Entity<Jogador>()
                .HasOne(j => j.Competicao)
                .WithMany(c => c.Jogadores)
                .HasForeignKey(j => j.CompeticaoId)
                .OnDelete(DeleteBehavior.Cascade);

            // JogoEmparelhado
            modelBuilder.Entity<JogoEmparelhado>()
                .HasKey(je => je.Id);
            modelBuilder.Entity<JogoEmparelhado>()
                .HasOne(je => je.Jogador1)
                .WithMany()
                .HasForeignKey(je => je.Jogador1Id)
                .OnDelete(DeleteBehavior.Restrict);
            modelBuilder.Entity<JogoEmparelhado>()
                .HasOne(je => je.Jogador2)
                .WithMany()
                .HasForeignKey(je => je.Jogador2Id)
                .OnDelete(DeleteBehavior.Restrict);
            modelBuilder.Entity<JogoEmparelhado>()
                .HasOne(je => je.Competicao)
                .WithMany()
                .HasForeignKey(je => je.CompeticaoId)
                .OnDelete(DeleteBehavior.Cascade);

            // EmparelhamentoBase
            modelBuilder.Entity<EmparelhamentoBase>()
                .HasKey(eb => eb.Id);
            modelBuilder.Entity<EmparelhamentoBase>()
                .Property(eb => eb.Clube1).IsRequired();
            modelBuilder.Entity<EmparelhamentoBase>()
                .Property(eb => eb.Clube2).IsRequired();
            modelBuilder.Entity<EmparelhamentoBase>()
                .Property(eb => eb.DataJogo).IsRequired();
            modelBuilder.Entity<EmparelhamentoBase>()
                .Property(eb => eb.HoraJogo).IsRequired();
            modelBuilder.Entity<EmparelhamentoBase>()
                .HasOne(eb => eb.Competicao)
                .WithMany()
                .HasForeignKey(eb => eb.CompeticaoId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
