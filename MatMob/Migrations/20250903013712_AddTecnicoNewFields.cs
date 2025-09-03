using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MatMob.Migrations
{
    /// <inheritdoc />
    public partial class AddTecnicoNewFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "CPF",
                table: "Tecnicos",
                type: "varchar(14)",
                maxLength: 14,
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<DateTime>(
                name: "DataAdmissao",
                table: "Tecnicos",
                type: "datetime(6)",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "DataNascimento",
                table: "Tecnicos",
                type: "datetime(6)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Especialidade",
                table: "Tecnicos",
                type: "varchar(100)",
                maxLength: 100,
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CPF",
                table: "Tecnicos");

            migrationBuilder.DropColumn(
                name: "DataAdmissao",
                table: "Tecnicos");

            migrationBuilder.DropColumn(
                name: "DataNascimento",
                table: "Tecnicos");

            migrationBuilder.DropColumn(
                name: "Especialidade",
                table: "Tecnicos");
        }
    }
}
