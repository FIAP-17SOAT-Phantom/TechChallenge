using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OficinaMecanica.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddAlertasEstoque : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AlertasEstoque",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    PecaId = table.Column<Guid>(type: "uuid", nullable: false),
                    NomePeca = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    QuantidadeDisponivel = table.Column<int>(type: "integer", nullable: false),
                    QuantidadeMinima = table.Column<int>(type: "integer", nullable: false),
                    DataCriacao = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    DataVisualizacao = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DataResolucao = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AlertasEstoque", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AlertasEstoque_DataResolucao",
                table: "AlertasEstoque",
                column: "DataResolucao");

            migrationBuilder.CreateIndex(
                name: "IX_AlertasEstoque_PecaId",
                table: "AlertasEstoque",
                column: "PecaId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AlertasEstoque");
        }
    }
}
