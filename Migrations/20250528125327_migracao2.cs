using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ProjetoLaboratorio25.Migrations
{
    /// <inheritdoc />
    public partial class migracao2 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Competicoes",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Nome = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    TipoCompeticao = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    NumJogadores = table.Column<int>(type: "int", nullable: false),
                    NumEquipas = table.Column<int>(type: "int", nullable: false),
                    PontosVitoria = table.Column<int>(type: "int", nullable: false),
                    PontosEmpate = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Competicoes", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Notificacoes",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Clube1 = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Clube2 = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    ClubeVitorioso = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Motivo = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    DataNotificacao = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Pontuacao1 = table.Column<int>(type: "int", nullable: false),
                    Pontuacao2 = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Notificacoes", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Reports",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Categoria = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Codigo = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Titulo = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Conteudo = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    DataCriacao = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Reports", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Utilizadores",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UtilizadorNome = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Email = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Senha = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Utilizadores", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ConfiguracoesFase",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    FaseNumero = table.Column<int>(type: "int", nullable: false),
                    NumJogosPorFase = table.Column<int>(type: "int", nullable: false),
                    Formato = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    PontosVitoria = table.Column<int>(type: "int", nullable: false),
                    PontosEmpate = table.Column<int>(type: "int", nullable: false),
                    PontosDerrota = table.Column<int>(type: "int", nullable: false),
                    PontosFaltaComparencia = table.Column<int>(type: "int", nullable: false),
                    PontosDesclassificacao = table.Column<int>(type: "int", nullable: false),
                    PontosExtra = table.Column<int>(type: "int", nullable: false),
                    CriteriosDesempate = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CompeticaoId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ConfiguracoesFase", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ConfiguracoesFase_Competicoes_CompeticaoId",
                        column: x => x.CompeticaoId,
                        principalTable: "Competicoes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "EmparelhamentosFinal",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Clube1 = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Clube2 = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    DataJogo = table.Column<DateTime>(type: "datetime2", nullable: false),
                    HoraJogo = table.Column<TimeSpan>(type: "time", nullable: false),
                    CompeticaoId = table.Column<int>(type: "int", nullable: false),
                    PontuacaoClube1 = table.Column<int>(type: "int", nullable: true),
                    PontuacaoClube2 = table.Column<int>(type: "int", nullable: true),
                    JogoRealizado = table.Column<bool>(type: "bit", nullable: false),
                    Motivo = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EmparelhamentosFinal", x => x.Id);
                    table.ForeignKey(
                        name: "FK_EmparelhamentosFinal_Competicoes_CompeticaoId",
                        column: x => x.CompeticaoId,
                        principalTable: "Competicoes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Jogadores",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Nome = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Codigo = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    DataNascimento = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Categoria = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Clube = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CompeticaoId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Jogadores", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Jogadores_Competicoes_CompeticaoId",
                        column: x => x.CompeticaoId,
                        principalTable: "Competicoes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ConfiguracoesFase_CompeticaoId",
                table: "ConfiguracoesFase",
                column: "CompeticaoId");

            migrationBuilder.CreateIndex(
                name: "IX_EmparelhamentosFinal_CompeticaoId",
                table: "EmparelhamentosFinal",
                column: "CompeticaoId");

            migrationBuilder.CreateIndex(
                name: "IX_Jogadores_CompeticaoId",
                table: "Jogadores",
                column: "CompeticaoId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ConfiguracoesFase");

            migrationBuilder.DropTable(
                name: "EmparelhamentosFinal");

            migrationBuilder.DropTable(
                name: "Jogadores");

            migrationBuilder.DropTable(
                name: "Notificacoes");

            migrationBuilder.DropTable(
                name: "Reports");

            migrationBuilder.DropTable(
                name: "Utilizadores");

            migrationBuilder.DropTable(
                name: "Competicoes");
        }
    }
}
