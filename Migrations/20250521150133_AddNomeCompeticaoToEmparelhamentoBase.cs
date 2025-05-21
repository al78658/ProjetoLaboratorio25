using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ProjetoLaboratorio25.Migrations
{
    /// <inheritdoc />
    public partial class AddNomeCompeticaoToEmparelhamentoBase : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "EmparelhamentosEquipa");

            migrationBuilder.CreateTable(
                name: "EmparelhamentosBase",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Clube1 = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Clube2 = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    DataJogo = table.Column<DateTime>(type: "datetime2", nullable: false),
                    HoraJogo = table.Column<TimeSpan>(type: "time", nullable: false),
                    CompeticaoId = table.Column<int>(type: "int", nullable: false),
                    NomeCompeticao = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EmparelhamentosBase", x => x.Id);
                    table.ForeignKey(
                        name: "FK_EmparelhamentosBase_Competicoes_CompeticaoId",
                        column: x => x.CompeticaoId,
                        principalTable: "Competicoes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_EmparelhamentosBase_CompeticaoId",
                table: "EmparelhamentosBase",
                column: "CompeticaoId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "EmparelhamentosBase");

            migrationBuilder.CreateTable(
                name: "EmparelhamentosEquipa",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CompeticaoId = table.Column<int>(type: "int", nullable: false),
                    Clube1 = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Clube2 = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    DataJogo = table.Column<DateTime>(type: "datetime2", nullable: false),
                    HoraJogo = table.Column<TimeSpan>(type: "time", nullable: false),
                    NomeCompeticao = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EmparelhamentosEquipa", x => x.Id);
                    table.ForeignKey(
                        name: "FK_EmparelhamentosEquipa_Competicoes_CompeticaoId",
                        column: x => x.CompeticaoId,
                        principalTable: "Competicoes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_EmparelhamentosEquipa_CompeticaoId",
                table: "EmparelhamentosEquipa",
                column: "CompeticaoId");
        }
    }
}
