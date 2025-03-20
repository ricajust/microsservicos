using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Alunos.API.Migrations
{
    /// <inheritdoc />
    public partial class AddUniqueIndexToCpf : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Usuarios_Usuarios_AlunoId",
                table: "Usuarios");

            migrationBuilder.DropIndex(
                name: "IX_Usuarios_AlunoId",
                table: "Usuarios");

            migrationBuilder.DropColumn(
                name: "AlunoId",
                table: "Usuarios");

            migrationBuilder.CreateIndex(
                name: "IX_Usuarios_cpf",
                table: "Usuarios",
                column: "cpf",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Usuarios_cpf",
                table: "Usuarios");

            migrationBuilder.AddColumn<Guid>(
                name: "AlunoId",
                table: "Usuarios",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Usuarios_AlunoId",
                table: "Usuarios",
                column: "AlunoId");

            migrationBuilder.AddForeignKey(
                name: "FK_Usuarios_Usuarios_AlunoId",
                table: "Usuarios",
                column: "AlunoId",
                principalTable: "Usuarios",
                principalColumn: "Id");
        }
    }
}
