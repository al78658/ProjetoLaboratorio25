using Microsoft.EntityFrameworkCore;
using ProjetoLaboratorio25.Models;
using System.Collections.Generic;

namespace ProjetoLaboratorio25.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<Competicao> Competicoes { get; set; }
        public DbSet<Jogador> Jogadores { get; set; }
        public DbSet<EmparelhamentoFinal> EmparelhamentosFinal { get; set; }
        public DbSet<ConfiguracaoFase> ConfiguracoesFase { get; set; }
        public DbSet<Report> Reports { get; set; }
        public DbSet<Notificacao> Notificacoes { get; set; }
        public DbSet<Utilizador> Utilizadores { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configure Utilizador
            modelBuilder.Entity<Utilizador>()
                .HasKey(u => u.Id);

            modelBuilder.Entity<Utilizador>()
                .Property(u => u.UtilizadorNome)
                .IsRequired()
                .HasMaxLength(100);

            modelBuilder.Entity<Utilizador>()
                .Property(u => u.Email)
                .IsRequired()
                .HasMaxLength(100);

            modelBuilder.Entity<Utilizador>()
                .Property(u => u.Senha)
                .IsRequired()
                .HasMaxLength(100);

            // Configure Competicao
            modelBuilder.Entity<Competicao>()
                .HasKey(c => c.Id);
                
            modelBuilder.Entity<Competicao>()
                .Property(c => c.Nome)
                .IsRequired()
                .HasMaxLength(100);
                
            modelBuilder.Entity<Competicao>()
                .Property(c => c.TipoCompeticao)
                .IsRequired()
                .HasMaxLength(50);
                
            modelBuilder.Entity<Competicao>()
                .Property(c => c.NumJogadores)
                .IsRequired();
                
            modelBuilder.Entity<Competicao>()
                .Property(c => c.NumEquipas)
                .IsRequired();
                
            modelBuilder.Entity<Competicao>()
                .Property(c => c.PontosVitoria)
                .IsRequired();
                
            modelBuilder.Entity<Competicao>()
                .Property(c => c.PontosEmpate)
                .IsRequired();

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
                
            // Configure Jogador
            modelBuilder.Entity<Jogador>()
                .HasKey(j => j.Id);
                
            modelBuilder.Entity<Jogador>()
                .Property(j => j.Nome)
                .IsRequired();
                
            modelBuilder.Entity<Jogador>()
                .Property(j => j.Codigo)
                .IsRequired();
                
            modelBuilder.Entity<Jogador>()
                .Property(j => j.DataNascimento)
                .IsRequired();
                
            modelBuilder.Entity<Jogador>()
                .Property(j => j.Categoria)
                .IsRequired();
                
            modelBuilder.Entity<Jogador>()
                .Property(j => j.Clube)
                .IsRequired();

            // Configure EmparelhamentoFinal
            modelBuilder.Entity<EmparelhamentoFinal>()
                .HasKey(ef => ef.Id);
                
            modelBuilder.Entity<EmparelhamentoFinal>()
                .HasOne(ef => ef.Competicao)
                .WithMany(c => c.EmparelhamentosFinal)
                .HasForeignKey(ef => ef.CompeticaoId)
                .OnDelete(DeleteBehavior.Cascade);

            // Configure Notificacao
            modelBuilder.Entity<Notificacao>(entity =>
            {
                entity.HasKey(n => n.Id);

                entity.Property(n => n.Clube1)
                    .IsRequired()
                    .HasMaxLength(100);

                entity.Property(n => n.Clube2)
                    .IsRequired()
                    .HasMaxLength(100);

                entity.Property(n => n.ClubeVitorioso)
                    .HasMaxLength(100);

                entity.Property(n => n.Motivo)
                    .HasMaxLength(500);

                entity.Property(n => n.DataNotificacao)
                    .IsRequired();
            });
        }
    }
}