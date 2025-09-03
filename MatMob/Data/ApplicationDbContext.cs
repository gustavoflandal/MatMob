using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using MatMob.Models.Entities;

namespace MatMob.Data
{
    public class ApplicationDbContext : IdentityDbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        // DbSets para as entidades
        public DbSet<Ativo> Ativos { get; set; }
        public DbSet<OrdemServico> OrdensServico { get; set; }
        public DbSet<Tecnico> Tecnicos { get; set; }
        public DbSet<Equipe> Equipes { get; set; }
        public DbSet<EquipeTecnico> EquipesTecnico { get; set; }
        public DbSet<Peca> Pecas { get; set; }
        public DbSet<ItemOrdemServico> ItensOrdemServico { get; set; }
        public DbSet<MovimentacaoEstoque> MovimentacoesEstoque { get; set; }
        public DbSet<PlanoManutencao> PlanosManutencao { get; set; }
    public DbSet<MaterialOrdemServico> MateriaisOrdemServico { get; set; }
    public DbSet<ApontamentoHoras> ApontamentosHoras { get; set; }
    public DbSet<AgendaManutencao> AgendaManutencao { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            // Configurações específicas das entidades
            
            // Ativo
            builder.Entity<Ativo>(entity =>
            {
                entity.HasIndex(e => e.NumeroSerie).IsUnique();
                entity.Property(e => e.Status).HasConversion<int>();
                // entity.Property(e => e.DataCadastro).HasDefaultValueSql("CURRENT_TIMESTAMP");
            });

            // OrdemServico
            builder.Entity<OrdemServico>(entity =>
            {
                entity.HasIndex(e => e.NumeroOS).IsUnique();
                entity.Property(e => e.TipoServico).HasConversion<int>();
                entity.Property(e => e.Status).HasConversion<int>();
                entity.Property(e => e.Prioridade).HasConversion<int>();
                // entity.Property(e => e.DataAbertura).HasDefaultValueSql("CURRENT_TIMESTAMP");
                
                // Relacionamentos
                entity.HasOne(d => d.Ativo)
                    .WithMany(p => p.OrdensServico)
                    .HasForeignKey(d => d.AtivoId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(d => d.Equipe)
                    .WithMany(p => p.OrdensServico)
                    .HasForeignKey(d => d.EquipeId)
                    .OnDelete(DeleteBehavior.SetNull);

                entity.HasOne(d => d.TecnicoResponsavel)
                    .WithMany(p => p.OrdensServicoResponsavel)
                    .HasForeignKey(d => d.TecnicoResponsavelId)
                    .OnDelete(DeleteBehavior.SetNull);
            });

            // Tecnico
            builder.Entity<Tecnico>(entity =>
            {
                entity.HasIndex(e => e.Email).IsUnique();
                entity.Property(e => e.Status).HasConversion<int>();
                // entity.Property(e => e.DataCadastro).HasDefaultValueSql("CURRENT_TIMESTAMP");
            });

            // Equipe
            builder.Entity<Equipe>(entity =>
            {
                entity.Property(e => e.Status).HasConversion<int>();
                // entity.Property(e => e.DataCriacao).HasDefaultValueSql("CURRENT_TIMESTAMP");
            });

            // EquipeTecnico - Relacionamento muitos para muitos
            builder.Entity<EquipeTecnico>(entity =>
            {
                entity.HasKey(e => e.Id);
                entity.HasIndex(e => new { e.EquipeId, e.TecnicoId, e.Ativo }).IsUnique();
                // entity.Property(e => e.DataEntrada).HasDefaultValueSql("CURRENT_TIMESTAMP");

                entity.HasOne(d => d.Equipe)
                    .WithMany(p => p.EquipesTecnico)
                    .HasForeignKey(d => d.EquipeId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(d => d.Tecnico)
                    .WithMany(p => p.EquipesTecnico)
                    .HasForeignKey(d => d.TecnicoId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            // Peca
            builder.Entity<Peca>(entity =>
            {
                entity.HasIndex(e => e.Codigo).IsUnique();
                // entity.Property(e => e.DataCadastro).HasDefaultValueSql("CURRENT_TIMESTAMP");
            });

            // ItemOrdemServico
            builder.Entity<ItemOrdemServico>(entity =>
            {
                entity.HasOne(d => d.OrdemServico)
                    .WithMany(p => p.ItensUtilizados)
                    .HasForeignKey(d => d.OrdemServicoId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(d => d.Peca)
                    .WithMany(p => p.ItensOrdemServico)
                    .HasForeignKey(d => d.PecaId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            // MovimentacaoEstoque
            builder.Entity<MovimentacaoEstoque>(entity =>
            {
                entity.Property(e => e.TipoMovimentacao).HasConversion<int>();
                // entity.Property(e => e.DataMovimentacao).HasDefaultValueSql("CURRENT_TIMESTAMP");

                entity.HasOne(d => d.Peca)
                    .WithMany(p => p.MovimentacoesEstoque)
                    .HasForeignKey(d => d.PecaId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(d => d.OrdemServico)
                    .WithMany()
                    .HasForeignKey(d => d.OrdemServicoId)
                    .OnDelete(DeleteBehavior.SetNull);
            });

            // PlanoManutencao
            builder.Entity<PlanoManutencao>(entity =>
            {
                entity.Property(e => e.Periodicidade).HasConversion<int>();
                // entity.Property(e => e.DataCriacao).HasDefaultValueSql("CURRENT_TIMESTAMP");

                entity.HasOne(d => d.Ativo)
                    .WithMany(p => p.PlanosManutencao)
                    .HasForeignKey(d => d.AtivoId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            // MaterialOrdemServico
            builder.Entity<MaterialOrdemServico>(entity =>
            {
                entity.HasOne(d => d.OrdemServico)
                    .WithMany(p => p.MateriaisUtilizados)
                    .HasForeignKey(d => d.OrdemServicoId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(d => d.Peca)
                    .WithMany()
                    .HasForeignKey(d => d.PecaId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            // ApontamentoHoras
            builder.Entity<ApontamentoHoras>(entity =>
            {
                entity.HasOne(d => d.OrdemServico)
                    .WithMany(p => p.ApontamentosHoras)
                    .HasForeignKey(d => d.OrdemServicoId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(d => d.Tecnico)
                    .WithMany()
                    .HasForeignKey(d => d.TecnicoId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            // AgendaManutencao
            builder.Entity<AgendaManutencao>(entity =>
            {
                entity.Property(e => e.Tipo).HasConversion<int>();
                entity.Property(e => e.Status).HasConversion<int>();

                entity.HasOne(d => d.Ativo)
                    .WithMany()
                    .HasForeignKey(d => d.AtivoId)
                    .OnDelete(DeleteBehavior.SetNull);

                entity.HasOne(d => d.OrdemServico)
                    .WithMany(p => p.AgendaItens)
                    .HasForeignKey(d => d.OrdemServicoId)
                    .OnDelete(DeleteBehavior.SetNull);

                entity.HasOne(d => d.Equipe)
                    .WithMany()
                    .HasForeignKey(d => d.EquipeId)
                    .OnDelete(DeleteBehavior.SetNull);

                entity.HasOne(d => d.Responsavel)
                    .WithMany()
                    .HasForeignKey(d => d.ResponsavelId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            // Seed data inicial
            SeedData(builder);
        }

        private static void SeedData(ModelBuilder builder)
        {
            // Data fixa para evitar problemas de migração
            var dataFixa = new DateTime(2025, 9, 1, 21, 30, 16, DateTimeKind.Utc);
            
            // Seed de tipos de ativos comuns
            builder.Entity<Ativo>().HasData(
                new Ativo 
                { 
                    Id = 1, 
                    Nome = "Semáforo Principal - Centro", 
                    Tipo = "Semáforo", 
                    Localizacao = "Rua Principal com Av. Central", 
                    Status = StatusAtivo.Ativo,
                    DataInstalacao = new DateTime(2023, 9, 1, 21, 30, 16, DateTimeKind.Utc),
                    DataCadastro = dataFixa
                },
                new Ativo 
                { 
                    Id = 2, 
                    Nome = "Radar Velocidade - BR101", 
                    Tipo = "Radar", 
                    Localizacao = "BR-101, KM 45", 
                    Status = StatusAtivo.Ativo,
                    DataInstalacao = new DateTime(2024, 9, 1, 21, 30, 16, DateTimeKind.Utc),
                    DataCadastro = dataFixa
                }
            );

            // Seed de peças comuns
            builder.Entity<Peca>().HasData(
                new Peca 
                { 
                    Id = 1, 
                    Codigo = "LED001", 
                    Nome = "Lâmpada LED Semáforo Vermelha", 
                    UnidadeMedida = "UN",
                    QuantidadeEstoque = 50,
                    EstoqueMinimo = 10,
                    PrecoUnitario = 25.90m,
                    Ativa = true,
                    DataCadastro = dataFixa
                },
                new Peca 
                { 
                    Id = 2, 
                    Codigo = "LED002", 
                    Nome = "Lâmpada LED Semáforo Amarela", 
                    UnidadeMedida = "UN",
                    QuantidadeEstoque = 30,
                    EstoqueMinimo = 10,
                    PrecoUnitario = 25.90m,
                    Ativa = true,
                    DataCadastro = dataFixa
                },
                new Peca 
                { 
                    Id = 3, 
                    Codigo = "LED003", 
                    Nome = "Lâmpada LED Semáforo Verde", 
                    UnidadeMedida = "UN",
                    QuantidadeEstoque = 40,
                    EstoqueMinimo = 10,
                    PrecoUnitario = 25.90m,
                    Ativa = true,
                    DataCadastro = dataFixa
                }
            );
        }
    }
}
