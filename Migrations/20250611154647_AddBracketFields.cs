using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ProjetoLaboratorio25.Migrations
{
    /// <inheritdoc />
    public partial class AddBracketFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Bracket",
                table: "EmparelhamentosFinal",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "RondaBracket",
                table: "EmparelhamentosFinal",
                type: "int",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Bracket",
                table: "EmparelhamentosFinal");

            migrationBuilder.DropColumn(
                name: "RondaBracket",
                table: "EmparelhamentosFinal");
        }
    }
}
