using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MatMob.Migrations
{
    /// <inheritdoc />
    public partial class AddProdutoFornecedorFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "CodigoFornecedor",
                table: "ProdutosFornecedores",
                type: "varchar(50)",
                maxLength: 50,
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "ModoFaturamento",
                table: "ProdutosFornecedores",
                type: "varchar(50)",
                maxLength: 50,
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<decimal>(
                name: "QuantidadeEmbalagem",
                table: "ProdutosFornecedores",
                type: "decimal(10,2)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CodigoFornecedor",
                table: "ProdutosFornecedores");

            migrationBuilder.DropColumn(
                name: "ModoFaturamento",
                table: "ProdutosFornecedores");

            migrationBuilder.DropColumn(
                name: "QuantidadeEmbalagem",
                table: "ProdutosFornecedores");
        }
    }
}
