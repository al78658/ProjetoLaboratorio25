using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ProjetoLaboratorio25.Migrations
{
    /// <inheritdoc />
    public partial class AddOrganizadorToCompeticao : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "OrganizadorId",
                table: "Competicoes",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Competicoes_OrganizadorId",
                table: "Competicoes",
                column: "OrganizadorId");

            migrationBuilder.AddForeignKey(
                name: "FK_Competicoes_Utilizadores_OrganizadorId",
                table: "Competicoes",
                column: "OrganizadorId",
                principalTable: "Utilizadores",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Competicoes_Utilizadores_OrganizadorId",
                table: "Competicoes");

            migrationBuilder.DropIndex(
                name: "IX_Competicoes_OrganizadorId",
                table: "Competicoes");

            migrationBuilder.DropColumn(
                name: "OrganizadorId",
                table: "Competicoes");
        }
    }
}
