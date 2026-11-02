using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace OficinaMecanica.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddAberturaCompletaOrdemDeServico : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateSequence(
                name: "ordem_servico_numero_seq");

            migrationBuilder.Sql("SELECT setval('ordem_servico_numero_seq', GREATEST((SELECT COALESCE(MAX(CAST(SUBSTRING(\"Numero\" FROM 4) AS bigint)), 0) FROM \"OrdensDeServico\" WHERE \"Numero\" ~ '^OS-[0-9]+$') + 1, 1), false);");

            migrationBuilder.CreateTable(
                name: "PecasSolicitadasOrdemDeServico",
                columns: table => new
                {
                    OrdemDeServicoId = table.Column<Guid>(type: "uuid", nullable: false),
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    PecaId = table.Column<Guid>(type: "uuid", nullable: false),
                    Quantidade = table.Column<int>(type: "integer", nullable: false),
                    Observacao = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PecasSolicitadasOrdemDeServico", x => new { x.OrdemDeServicoId, x.Id });
                    table.CheckConstraint("CK_PecasSolicitadasOrdemDeServico_Quantidade", "\"Quantidade\" > 0");
                    table.ForeignKey(
                        name: "FK_PecasSolicitadasOrdemDeServico_OrdensDeServico_OrdemDeServi~",
                        column: x => x.OrdemDeServicoId,
                        principalTable: "OrdensDeServico",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_PecasSolicitadasOrdemDeServico_Pecas_PecaId",
                        column: x => x.PecaId,
                        principalTable: "Pecas",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ServicosSolicitadosOrdemDeServico",
                columns: table => new
                {
                    OrdemDeServicoId = table.Column<Guid>(type: "uuid", nullable: false),
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ServicoId = table.Column<Guid>(type: "uuid", nullable: false),
                    Quantidade = table.Column<int>(type: "integer", nullable: false),
                    Observacao = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ServicosSolicitadosOrdemDeServico", x => new { x.OrdemDeServicoId, x.Id });
                    table.CheckConstraint("CK_ServicosSolicitadosOrdemDeServico_Quantidade", "\"Quantidade\" > 0");
                    table.ForeignKey(
                        name: "FK_ServicosSolicitadosOrdemDeServico_OrdensDeServico_OrdemDeSe~",
                        column: x => x.OrdemDeServicoId,
                        principalTable: "OrdensDeServico",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ServicosSolicitadosOrdemDeServico_Servicos_ServicoId",
                        column: x => x.ServicoId,
                        principalTable: "Servicos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_PecasSolicitadasOrdemDeServico_OrdemDeServicoId_PecaId",
                table: "PecasSolicitadasOrdemDeServico",
                columns: new[] { "OrdemDeServicoId", "PecaId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_PecasSolicitadasOrdemDeServico_PecaId",
                table: "PecasSolicitadasOrdemDeServico",
                column: "PecaId");

            migrationBuilder.CreateIndex(
                name: "IX_ServicosSolicitadosOrdemDeServico_OrdemDeServicoId_ServicoId",
                table: "ServicosSolicitadosOrdemDeServico",
                columns: new[] { "OrdemDeServicoId", "ServicoId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ServicosSolicitadosOrdemDeServico_ServicoId",
                table: "ServicosSolicitadosOrdemDeServico",
                column: "ServicoId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "PecasSolicitadasOrdemDeServico");

            migrationBuilder.DropTable(
                name: "ServicosSolicitadosOrdemDeServico");

            migrationBuilder.DropSequence(
                name: "ordem_servico_numero_seq");
        }
    }
}
