using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MatMob.Migrations
{
    /// <inheritdoc />
    public partial class AddSolicitanteIdToOrdemServico : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "SolicitanteId",
                table: "OrdensServico",
                type: "int",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "SolicitanteId",
                table: "OrdensServico");
        }
    }
}
