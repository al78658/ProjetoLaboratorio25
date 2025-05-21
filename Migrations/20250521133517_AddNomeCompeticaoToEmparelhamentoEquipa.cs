using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ProjetoLaboratorio25.Migrations
{
    /// <inheritdoc />
    public partial class AddNomeCompeticaoToEmparelhamentoEquipa : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "NomeCompeticao",
                table: "EmparelhamentosEquipa",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "NomeCompeticao",
                table: "EmparelhamentosEquipa");
        }
    }
}
