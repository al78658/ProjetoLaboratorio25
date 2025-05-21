using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ProjetoLaboratorio25.Migrations
{
    /// <inheritdoc />
    public partial class AddCompeticaoIdToEmparelhamentoEquipa : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        { 
            migrationBuilder.AddColumn<int>(
            name: "CompeticaoId1",
            table: "JogosEmparelhados",
            type: "int",
            nullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Nome",
                table: "Jogadores",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(100)",
                oldMaxLength: 100);

            migrationBuilder.AlterColumn<string>(
                name: "Codigo",
                table: "Jogadores",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(10)",
                oldMaxLength: 10);

            migrationBuilder.AlterColumn<string>(
                name: "Clube",
                table: "Jogadores",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(100)",
                oldMaxLength: 100);

            migrationBuilder.AlterColumn<string>(
                name: "Categoria",
                table: "Jogadores",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(50)",
                oldMaxLength: 50);

            migrationBuilder.AddColumn<int>(
                name: "CompeticaoId",
                table: "Jogadores",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateTable(
                name: "EmparelhamentosEquipa",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Clube1 = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Clube2 = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CompeticaoId = table.Column<int>(type: "int", nullable: false),
                    DataJogo = table.Column<DateTime>(type: "datetime2", nullable: false),
                    HoraJogo = table.Column<TimeSpan>(type: "time", nullable: false)
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
                name: "IX_JogosEmparelhados_CompeticaoId",
                table: "JogosEmparelhados",
                column: "CompeticaoId");

            migrationBuilder.CreateIndex(
                name: "IX_JogosEmparelhados_CompeticaoId1",
                table: "JogosEmparelhados",
                column: "CompeticaoId1");

            migrationBuilder.CreateIndex(
                name: "IX_Jogadores_CompeticaoId",
                table: "Jogadores",
                column: "CompeticaoId");

            migrationBuilder.CreateIndex(
                name: "IX_EmparelhamentosEquipa_CompeticaoId",
                table: "EmparelhamentosEquipa",
                column: "CompeticaoId");

            migrationBuilder.AddForeignKey(
                name: "FK_Jogadores_Competicoes_CompeticaoId",
                table: "Jogadores",
                column: "CompeticaoId",
                principalTable: "Competicoes",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_JogosEmparelhados_Competicoes_CompeticaoId",
                table: "JogosEmparelhados",
                column: "CompeticaoId",
                principalTable: "Competicoes",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_JogosEmparelhados_Competicoes_CompeticaoId1",
                table: "JogosEmparelhados",
                column: "CompeticaoId1",
                principalTable: "Competicoes",
                principalColumn: "Id");

            // Adicionar a coluna CompeticaoId à tabela EmparelhamentosEquipa
            migrationBuilder.AddColumn<int>(
                name: "CompeticaoId",
                table: "EmparelhamentosEquipa",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Jogadores_Competicoes_CompeticaoId",
                table: "Jogadores");

            migrationBuilder.DropForeignKey(
                name: "FK_JogosEmparelhados_Competicoes_CompeticaoId",
                table: "JogosEmparelhados");

            migrationBuilder.DropForeignKey(
                name: "FK_JogosEmparelhados_Competicoes_CompeticaoId1",
                table: "JogosEmparelhados");

            migrationBuilder.DropTable(
                name: "EmparelhamentosEquipa");

            migrationBuilder.DropIndex(
                name: "IX_JogosEmparelhados_CompeticaoId",
                table: "JogosEmparelhados");

            migrationBuilder.DropIndex(
                name: "IX_JogosEmparelhados_CompeticaoId1",
                table: "JogosEmparelhados");

            migrationBuilder.DropIndex(
                name: "IX_Jogadores_CompeticaoId",
                table: "Jogadores");

            migrationBuilder.DropColumn(
                name: "CompeticaoId1",
                table: "JogosEmparelhados");

            migrationBuilder.DropColumn(
                name: "CompeticaoId",
                table: "Jogadores");

            migrationBuilder.AlterColumn<string>(
                name: "Nome",
                table: "Jogadores",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<string>(
                name: "Codigo",
                table: "Jogadores",
                type: "nvarchar(10)",
                maxLength: 10,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<string>(
                name: "Clube",
                table: "Jogadores",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<string>(
                name: "Categoria",
                table: "Jogadores",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");
        }
    }
}
