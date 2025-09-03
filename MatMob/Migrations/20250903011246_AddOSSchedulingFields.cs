using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MatMob.Migrations
{
    /// <inheritdoc />
    public partial class AddOSSchedulingFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "DataProgramada",
                table: "OrdensServico",
                type: "datetime(6)",
                nullable: true);

            migrationBuilder.AddColumn<TimeSpan>(
                name: "HoraFimProgramada",
                table: "OrdensServico",
                type: "time(6)",
                nullable: true);

            migrationBuilder.AddColumn<TimeSpan>(
                name: "HoraInicioProgramada",
                table: "OrdensServico",
                type: "time(6)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DataProgramada",
                table: "OrdensServico");

            migrationBuilder.DropColumn(
                name: "HoraFimProgramada",
                table: "OrdensServico");

            migrationBuilder.DropColumn(
                name: "HoraInicioProgramada",
                table: "OrdensServico");
        }
    }
}
