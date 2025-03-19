using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Alunos.API.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Usuarios",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    nome = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    cpf = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    data_nascimento = table.Column<DateTime>(type: "datetime2", nullable: true),
                    email = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    telefone = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    endereco = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    bairro = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    cidade = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    uf = table.Column<string>(type: "nvarchar(2)", maxLength: 2, nullable: true),
                    cep = table.Column<string>(type: "nvarchar(9)", maxLength: 9, nullable: true),
                    senha = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    AlunoId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    Discriminator = table.Column<string>(type: "nvarchar(8)", maxLength: 8, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Usuarios", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Usuarios_Usuarios_AlunoId",
                        column: x => x.AlunoId,
                        principalTable: "Usuarios",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_Usuarios_AlunoId",
                table: "Usuarios",
                column: "AlunoId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Usuarios");
        }
    }
}
