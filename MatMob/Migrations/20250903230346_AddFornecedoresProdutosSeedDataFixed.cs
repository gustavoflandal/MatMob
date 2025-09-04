using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace MatMob.Migrations
{
    /// <inheritdoc />
    public partial class AddFornecedoresProdutosSeedDataFixed : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "Fornecedores",
                columns: new[] { "Id", "Bairro", "CEP", "CNPJ", "Celular", "Cidade", "DataCadastro", "Email", "Endereco", "Estado", "InscricaoEstadual", "Nome", "NomeContato", "NomeFantasia", "Observacoes", "Status", "Telefone", "UltimaAtualizacao" },
                values: new object[,]
                {
                    { 100, "Centro", "01234-567", "12.345.678/0001-90", "(11) 99999-8888", "São Paulo", new DateTime(2025, 9, 1, 21, 30, 16, 0, DateTimeKind.Utc), "vendas@tecnoled.com.br", "Rua das Tecnologias, 123", "SP", "123456789", "TecnoLED Ltda", "João Silva", "TecnoLED", "Especialista em iluminação LED", 1, "(11) 3333-4444", null },
                    { 101, "Industrial", "20000-000", "98.765.432/0001-10", "(21) 88888-7777", "Rio de Janeiro", new DateTime(2025, 9, 1, 21, 30, 16, 0, DateTimeKind.Utc), "compras@sinalmod.com.br", "Av. dos Sinais, 456", "RJ", "987654321", "Sinalizações Modernas S.A.", "Maria Santos", "SinalMod", "Fornecedor de equipamentos de sinalização", 1, "(21) 2222-3333", null },
                    { 102, "Tecnológico", "30000-000", "11.222.333/0001-44", "(31) 77777-6666", "Belo Horizonte", new DateTime(2025, 9, 1, 21, 30, 16, 0, DateTimeKind.Utc), "vendas@compelet.com.br", "Rua dos Componentes, 789", "MG", "112223334", "Componentes Eletrônicos Ltda", "Pedro Costa", "CompElet", "Componentes eletrônicos diversos", 1, "(31) 1111-2222", null }
                });

            migrationBuilder.InsertData(
                table: "Produtos",
                columns: new[] { "Id", "Codigo", "DataCadastro", "Descricao", "EstoqueAtual", "EstoqueMinimo", "Nome", "Observacoes", "Status", "UltimaAtualizacao", "UnidadeMedida" },
                values: new object[,]
                {
                    { 100, "LED-VERM-001", new DateTime(2025, 9, 1, 21, 30, 16, 0, DateTimeKind.Utc), "Lâmpada LED vermelha para semáforos, 12V, alta durabilidade", 0m, 20m, "Lâmpada LED Vermelha 12V", "Produto essencial para manutenção de semáforos", 1, null, "UN" },
                    { 101, "LED-AMAR-001", new DateTime(2025, 9, 1, 21, 30, 16, 0, DateTimeKind.Utc), "Lâmpada LED amarela para semáforos, 12V, alta durabilidade", 0m, 20m, "Lâmpada LED Amarela 12V", "Produto essencial para manutenção de semáforos", 1, null, "UN" },
                    { 102, "LED-VERD-001", new DateTime(2025, 9, 1, 21, 30, 16, 0, DateTimeKind.Utc), "Lâmpada LED verde para semáforos, 12V, alta durabilidade", 0m, 20m, "Lâmpada LED Verde 12V", "Produto essencial para manutenção de semáforos", 1, null, "UN" },
                    { 103, "CONTROL-001", new DateTime(2025, 9, 1, 21, 30, 16, 0, DateTimeKind.Utc), "Controlador eletrônico para semáforos com timer programável", 0m, 5m, "Controlador de Semáforo", "Equipamento de controle para semáforos", 1, null, "UN" },
                    { 104, "SENSOR-001", new DateTime(2025, 9, 1, 21, 30, 16, 0, DateTimeKind.Utc), "Sensor infravermelho para detecção de veículos", 0m, 10m, "Sensor de Presença", "Sensor para otimização do tráfego", 1, null, "UN" }
                });

            migrationBuilder.InsertData(
                table: "ProdutosFornecedores",
                columns: new[] { "Id", "CodigoFornecedor", "CondicaoPagamento", "DataAtualizacao", "DataValidade", "FornecedorId", "ModoFaturamento", "Observacoes", "PrazoEntregaDias", "Preco", "ProdutoId", "QuantidadeEmbalagem", "Status" },
                values: new object[,]
                {
                    { 100, "TL-LED-VERM-12V", "30 dias", new DateTime(2025, 9, 1, 21, 30, 16, 0, DateTimeKind.Utc), new DateTime(2026, 3, 1, 21, 30, 16, 0, DateTimeKind.Utc), 100, "Caixa com 10 unidades", "Preço promocional para compras acima de 50 unidades", 7, 25.90m, 100, 10m, 1 },
                    { 101, "SM-LED-VERM-001", "15 dias", new DateTime(2025, 9, 1, 21, 30, 16, 0, DateTimeKind.Utc), new DateTime(2025, 12, 1, 21, 30, 16, 0, DateTimeKind.Utc), 101, "Caixa com 5 unidades", "Entrega rápida disponível", 5, 28.50m, 100, 5m, 1 },
                    { 102, "TL-LED-AMAR-12V", "30 dias", new DateTime(2025, 9, 1, 21, 30, 16, 0, DateTimeKind.Utc), new DateTime(2026, 3, 1, 21, 30, 16, 0, DateTimeKind.Utc), 100, "Caixa com 10 unidades", "Preço promocional para compras acima de 50 unidades", 7, 25.90m, 101, 10m, 1 },
                    { 103, "TL-LED-VERD-12V", "30 dias", new DateTime(2025, 9, 1, 21, 30, 16, 0, DateTimeKind.Utc), new DateTime(2026, 3, 1, 21, 30, 16, 0, DateTimeKind.Utc), 100, "Caixa com 10 unidades", "Preço promocional para compras acima de 50 unidades", 7, 25.90m, 102, 10m, 1 },
                    { 104, "CE-CONTROL-001", "45 dias", new DateTime(2025, 9, 1, 21, 30, 16, 0, DateTimeKind.Utc), new DateTime(2026, 9, 1, 21, 30, 16, 0, DateTimeKind.Utc), 102, "Unidade", "Inclui garantia de 2 anos", 15, 450.00m, 103, 1m, 1 },
                    { 105, "CE-SENSOR-IR-001", "30 dias", new DateTime(2025, 9, 1, 21, 30, 16, 0, DateTimeKind.Utc), new DateTime(2026, 9, 1, 21, 30, 16, 0, DateTimeKind.Utc), 102, "Unidade", "Sensor com alcance de 50 metros", 10, 120.00m, 104, 1m, 1 }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "ProdutosFornecedores",
                keyColumn: "Id",
                keyValue: 100);

            migrationBuilder.DeleteData(
                table: "ProdutosFornecedores",
                keyColumn: "Id",
                keyValue: 101);

            migrationBuilder.DeleteData(
                table: "ProdutosFornecedores",
                keyColumn: "Id",
                keyValue: 102);

            migrationBuilder.DeleteData(
                table: "ProdutosFornecedores",
                keyColumn: "Id",
                keyValue: 103);

            migrationBuilder.DeleteData(
                table: "ProdutosFornecedores",
                keyColumn: "Id",
                keyValue: 104);

            migrationBuilder.DeleteData(
                table: "ProdutosFornecedores",
                keyColumn: "Id",
                keyValue: 105);

            migrationBuilder.DeleteData(
                table: "Fornecedores",
                keyColumn: "Id",
                keyValue: 100);

            migrationBuilder.DeleteData(
                table: "Fornecedores",
                keyColumn: "Id",
                keyValue: 101);

            migrationBuilder.DeleteData(
                table: "Fornecedores",
                keyColumn: "Id",
                keyValue: 102);

            migrationBuilder.DeleteData(
                table: "Produtos",
                keyColumn: "Id",
                keyValue: 100);

            migrationBuilder.DeleteData(
                table: "Produtos",
                keyColumn: "Id",
                keyValue: 101);

            migrationBuilder.DeleteData(
                table: "Produtos",
                keyColumn: "Id",
                keyValue: 102);

            migrationBuilder.DeleteData(
                table: "Produtos",
                keyColumn: "Id",
                keyValue: 103);

            migrationBuilder.DeleteData(
                table: "Produtos",
                keyColumn: "Id",
                keyValue: 104);
        }
    }
}
