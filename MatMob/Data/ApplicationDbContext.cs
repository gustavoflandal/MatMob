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

        // Novas entidades para gestão de compras
        public DbSet<Fornecedor> Fornecedores { get; set; }
        public DbSet<Produto> Produtos { get; set; }
        public DbSet<ProdutoFornecedor> ProdutosFornecedores { get; set; }
        public DbSet<PedidoCompra> PedidosCompra { get; set; }
        public DbSet<ItemPedidoCompra> ItensPedidoCompra { get; set; }
        public DbSet<NotaFiscal> NotasFiscais { get; set; }
        public DbSet<ItemNotaFiscal> ItensNotaFiscal { get; set; }
        
        // Entidade para auditoria
        public DbSet<AuditLog> AuditLogs { get; set; }
        public DbSet<AuditModuleConfig> AuditModuleConfigs { get; set; } = null!;

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

            // Configurações das novas entidades

            // AuditLog
            builder.Entity<AuditLog>(entity =>
            {
                entity.HasIndex(e => e.CreatedAt);
                entity.HasIndex(e => e.UserId);
                entity.HasIndex(e => e.Action);
                entity.HasIndex(e => e.EntityName);
                entity.HasIndex(e => e.CorrelationId);
                entity.Property(e => e.CreatedAt)
                    .HasColumnType("timestamp(6)")
                    .HasDefaultValueSql("CURRENT_TIMESTAMP(6)");
            });

            // AuditModuleConfig
            builder.Entity<AuditModuleConfig>(entity =>
            {
                entity.HasIndex(e => new { e.Module, e.Process }).IsUnique();
                entity.Property(e => e.Module).IsRequired().HasMaxLength(100);
                entity.Property(e => e.Process).HasMaxLength(100);
                entity.Property(e => e.Enabled).HasDefaultValue(true);
                entity.Property(e => e.CreatedAt).HasDefaultValueSql("CURRENT_TIMESTAMP(6)");
            });

            // Fornecedor
            builder.Entity<Fornecedor>(entity =>
            {
                entity.HasIndex(e => e.CNPJ).IsUnique();
                entity.HasIndex(e => e.Email).IsUnique();
                entity.Property(e => e.Status).HasConversion<int>();
            });

            // Produto
            builder.Entity<Produto>(entity =>
            {
                entity.HasIndex(e => e.Codigo).IsUnique();
                entity.Property(e => e.Status).HasConversion<int>();
            });

            // ProdutoFornecedor
            builder.Entity<ProdutoFornecedor>(entity =>
            {
                entity.Property(e => e.Status).HasConversion<int>();

                entity.HasOne(d => d.Produto)
                    .WithMany(p => p.Fornecedores)
                    .HasForeignKey(d => d.ProdutoId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(d => d.Fornecedor)
                    .WithMany(p => p.ProdutosFornecidos)
                    .HasForeignKey(d => d.FornecedorId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            // PedidoCompra
            builder.Entity<PedidoCompra>(entity =>
            {
                entity.HasIndex(e => e.NumeroPedido).IsUnique();
                entity.Property(e => e.Status).HasConversion<int>();
                entity.Property(e => e.Prioridade).HasConversion<int>();

                entity.HasOne(d => d.Fornecedor)
                    .WithMany(p => p.PedidosCompra)
                    .HasForeignKey(d => d.FornecedorId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            // ItemPedidoCompra
            builder.Entity<ItemPedidoCompra>(entity =>
            {
                entity.HasOne(d => d.PedidoCompra)
                    .WithMany(p => p.Itens)
                    .HasForeignKey(d => d.PedidoCompraId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(d => d.Produto)
                    .WithMany(p => p.ItensPedidoCompra)
                    .HasForeignKey(d => d.ProdutoId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            // NotaFiscal
            builder.Entity<NotaFiscal>(entity =>
            {
                entity.HasIndex(e => new { e.NumeroNF, e.Serie }).IsUnique();

                entity.HasOne(d => d.PedidoCompra)
                    .WithMany(p => p.NotasFiscais)
                    .HasForeignKey(d => d.PedidoCompraId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(d => d.Fornecedor)
                    .WithMany()
                    .HasForeignKey(d => d.FornecedorId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            // ItemNotaFiscal
            builder.Entity<ItemNotaFiscal>(entity =>
            {
                entity.HasOne(d => d.NotaFiscal)
                    .WithMany(p => p.Itens)
                    .HasForeignKey(d => d.NotaFiscalId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(d => d.Produto)
                    .WithMany(p => p.ItensNotaFiscal)
                    .HasForeignKey(d => d.ProdutoId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(d => d.ItemPedidoCompra)
                    .WithMany(p => p.ItensNotaFiscal)
                    .HasForeignKey(d => d.ItemPedidoCompraId)
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

            // Seed de fornecedores
            builder.Entity<Fornecedor>().HasData(
                new Fornecedor 
                { 
                    Id = 100, 
                    Nome = "TecnoLED Ltda", 
                    NomeFantasia = "TecnoLED",
                    CNPJ = "12.345.678/0001-90",
                    InscricaoEstadual = "123456789",
                    Endereco = "Rua das Tecnologias, 123",
                    Bairro = "Centro",
                    Cidade = "São Paulo",
                    Estado = "SP",
                    CEP = "01234-567",
                    Telefone = "(11) 3333-4444",
                    Celular = "(11) 99999-8888",
                    Email = "vendas@tecnoled.com.br",
                    NomeContato = "João Silva",
                    Status = StatusFornecedor.Ativo,
                    Observacoes = "Especialista em iluminação LED",
                    DataCadastro = dataFixa
                },
                new Fornecedor 
                { 
                    Id = 101, 
                    Nome = "Sinalizações Modernas S.A.", 
                    NomeFantasia = "SinalMod",
                    CNPJ = "98.765.432/0001-10",
                    InscricaoEstadual = "987654321",
                    Endereco = "Av. dos Sinais, 456",
                    Bairro = "Industrial",
                    Cidade = "Rio de Janeiro",
                    Estado = "RJ",
                    CEP = "20000-000",
                    Telefone = "(21) 2222-3333",
                    Celular = "(21) 88888-7777",
                    Email = "compras@sinalmod.com.br",
                    NomeContato = "Maria Santos",
                    Status = StatusFornecedor.Ativo,
                    Observacoes = "Fornecedor de equipamentos de sinalização",
                    DataCadastro = dataFixa
                },
                new Fornecedor 
                { 
                    Id = 102, 
                    Nome = "Componentes Eletrônicos Ltda", 
                    NomeFantasia = "CompElet",
                    CNPJ = "11.222.333/0001-44",
                    InscricaoEstadual = "112223334",
                    Endereco = "Rua dos Componentes, 789",
                    Bairro = "Tecnológico",
                    Cidade = "Belo Horizonte",
                    Estado = "MG",
                    CEP = "30000-000",
                    Telefone = "(31) 1111-2222",
                    Celular = "(31) 77777-6666",
                    Email = "vendas@compelet.com.br",
                    NomeContato = "Pedro Costa",
                    Status = StatusFornecedor.Ativo,
                    Observacoes = "Componentes eletrônicos diversos",
                    DataCadastro = dataFixa
                }
            );

            // Seed de produtos
            builder.Entity<Produto>().HasData(
                new Produto 
                { 
                    Id = 100, 
                    Codigo = "LED-VERM-001", 
                    Nome = "Lâmpada LED Vermelha 12V",
                    Descricao = "Lâmpada LED vermelha para semáforos, 12V, alta durabilidade",
                    UnidadeMedida = "UN",
                    EstoqueMinimo = 20,
                    EstoqueAtual = 0,
                    Status = StatusProduto.Ativo,
                    Observacoes = "Produto essencial para manutenção de semáforos",
                    DataCadastro = dataFixa
                },
                new Produto 
                { 
                    Id = 101, 
                    Codigo = "LED-AMAR-001", 
                    Nome = "Lâmpada LED Amarela 12V",
                    Descricao = "Lâmpada LED amarela para semáforos, 12V, alta durabilidade",
                    UnidadeMedida = "UN",
                    EstoqueMinimo = 20,
                    EstoqueAtual = 0,
                    Status = StatusProduto.Ativo,
                    Observacoes = "Produto essencial para manutenção de semáforos",
                    DataCadastro = dataFixa
                },
                new Produto 
                { 
                    Id = 102, 
                    Codigo = "LED-VERD-001", 
                    Nome = "Lâmpada LED Verde 12V",
                    Descricao = "Lâmpada LED verde para semáforos, 12V, alta durabilidade",
                    UnidadeMedida = "UN",
                    EstoqueMinimo = 20,
                    EstoqueAtual = 0,
                    Status = StatusProduto.Ativo,
                    Observacoes = "Produto essencial para manutenção de semáforos",
                    DataCadastro = dataFixa
                },
                new Produto 
                { 
                    Id = 103, 
                    Codigo = "CONTROL-001", 
                    Nome = "Controlador de Semáforo",
                    Descricao = "Controlador eletrônico para semáforos com timer programável",
                    UnidadeMedida = "UN",
                    EstoqueMinimo = 5,
                    EstoqueAtual = 0,
                    Status = StatusProduto.Ativo,
                    Observacoes = "Equipamento de controle para semáforos",
                    DataCadastro = dataFixa
                },
                new Produto 
                { 
                    Id = 104, 
                    Codigo = "SENSOR-001", 
                    Nome = "Sensor de Presença",
                    Descricao = "Sensor infravermelho para detecção de veículos",
                    UnidadeMedida = "UN",
                    EstoqueMinimo = 10,
                    EstoqueAtual = 0,
                    Status = StatusProduto.Ativo,
                    Observacoes = "Sensor para otimização do tráfego",
                    DataCadastro = dataFixa
                }
            );

            // Seed de relacionamentos produto-fornecedor
            builder.Entity<ProdutoFornecedor>().HasData(
                new ProdutoFornecedor 
                { 
                    Id = 100, 
                    ProdutoId = 100, 
                    FornecedorId = 100, 
                    Preco = 25.90m,
                    CodigoFornecedor = "TL-LED-VERM-12V",
                    QuantidadeEmbalagem = 10,
                    ModoFaturamento = "Caixa com 10 unidades",
                    DataAtualizacao = dataFixa,
                    DataValidade = dataFixa.AddMonths(6),
                    CondicaoPagamento = "30 dias",
                    PrazoEntregaDias = 7,
                    Status = StatusProdutoFornecedor.Ativo,
                    Observacoes = "Preço promocional para compras acima de 50 unidades"
                },
                new ProdutoFornecedor 
                { 
                    Id = 101, 
                    ProdutoId = 100, 
                    FornecedorId = 101, 
                    Preco = 28.50m,
                    CodigoFornecedor = "SM-LED-VERM-001",
                    QuantidadeEmbalagem = 5,
                    ModoFaturamento = "Caixa com 5 unidades",
                    DataAtualizacao = dataFixa,
                    DataValidade = dataFixa.AddMonths(3),
                    CondicaoPagamento = "15 dias",
                    PrazoEntregaDias = 5,
                    Status = StatusProdutoFornecedor.Ativo,
                    Observacoes = "Entrega rápida disponível"
                },
                new ProdutoFornecedor 
                { 
                    Id = 102, 
                    ProdutoId = 101, 
                    FornecedorId = 100, 
                    Preco = 25.90m,
                    CodigoFornecedor = "TL-LED-AMAR-12V",
                    QuantidadeEmbalagem = 10,
                    ModoFaturamento = "Caixa com 10 unidades",
                    DataAtualizacao = dataFixa,
                    DataValidade = dataFixa.AddMonths(6),
                    CondicaoPagamento = "30 dias",
                    PrazoEntregaDias = 7,
                    Status = StatusProdutoFornecedor.Ativo,
                    Observacoes = "Preço promocional para compras acima de 50 unidades"
                },
                new ProdutoFornecedor 
                { 
                    Id = 103, 
                    ProdutoId = 102, 
                    FornecedorId = 100, 
                    Preco = 25.90m,
                    CodigoFornecedor = "TL-LED-VERD-12V",
                    QuantidadeEmbalagem = 10,
                    ModoFaturamento = "Caixa com 10 unidades",
                    DataAtualizacao = dataFixa,
                    DataValidade = dataFixa.AddMonths(6),
                    CondicaoPagamento = "30 dias",
                    PrazoEntregaDias = 7,
                    Status = StatusProdutoFornecedor.Ativo,
                    Observacoes = "Preço promocional para compras acima de 50 unidades"
                },
                new ProdutoFornecedor 
                { 
                    Id = 104, 
                    ProdutoId = 103, 
                    FornecedorId = 102, 
                    Preco = 450.00m,
                    CodigoFornecedor = "CE-CONTROL-001",
                    QuantidadeEmbalagem = 1,
                    ModoFaturamento = "Unidade",
                    DataAtualizacao = dataFixa,
                    DataValidade = dataFixa.AddMonths(12),
                    CondicaoPagamento = "45 dias",
                    PrazoEntregaDias = 15,
                    Status = StatusProdutoFornecedor.Ativo,
                    Observacoes = "Inclui garantia de 2 anos"
                },
                new ProdutoFornecedor 
                { 
                    Id = 105, 
                    ProdutoId = 104, 
                    FornecedorId = 102, 
                    Preco = 120.00m,
                    CodigoFornecedor = "CE-SENSOR-IR-001",
                    QuantidadeEmbalagem = 1,
                    ModoFaturamento = "Unidade",
                    DataAtualizacao = dataFixa,
                    DataValidade = dataFixa.AddMonths(12),
                    CondicaoPagamento = "30 dias",
                    PrazoEntregaDias = 10,
                    Status = StatusProdutoFornecedor.Ativo,
                    Observacoes = "Sensor com alcance de 50 metros"
                }
            );
        }
    }
}
