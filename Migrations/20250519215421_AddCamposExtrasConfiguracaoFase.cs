using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ProjetoLaboratorio25.Migrations
{
    /// <inheritdoc />
    public partial class AddCamposExtrasConfiguracaoFase : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "PontosDesclassificacao",
                table: "ConfiguracoesFase",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "PontosExtra",
                table: "ConfiguracoesFase",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "PontosFaltaComparencia",
                table: "ConfiguracoesFase",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "PontosDesclassificacao",
                table: "ConfiguracoesFase");

            migrationBuilder.DropColumn(
                name: "PontosExtra",
                table: "ConfiguracoesFase");

            migrationBuilder.DropColumn(
                name: "PontosFaltaComparencia",
                table: "ConfiguracoesFase");
        }
    }
}
