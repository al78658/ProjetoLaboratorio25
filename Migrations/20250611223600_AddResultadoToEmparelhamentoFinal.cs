using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ProjetoLaboratorio25.Migrations
{
    /// <inheritdoc />
    public partial class AddResultadoToEmparelhamentoFinal : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "Resultado",
                table: "EmparelhamentosFinal",
                type: "int",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Resultado",
                table: "EmparelhamentosFinal");
        }
    }
}
