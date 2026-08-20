using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OficinaMecanica.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddIntegridadeRelacional : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_AlertasEstoque_PecaId",
                table: "AlertasEstoque");

            migrationBuilder.CreateIndex(
                name: "IX_Orcamentos_OrdemDeServicoId_Versao",
                table: "Orcamentos",
                columns: new[] { "OrdemDeServicoId", "Versao" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ItensOrdemDeServico_PecaId",
                table: "ItensOrdemDeServico",
                column: "PecaId");

            migrationBuilder.CreateIndex(
                name: "IX_ItensOrdemDeServico_ServicoId",
                table: "ItensOrdemDeServico",
                column: "ServicoId");

            migrationBuilder.CreateIndex(
                name: "IX_ItensOrcamento_PecaId",
                table: "ItensOrcamento",
                column: "PecaId");

            migrationBuilder.CreateIndex(
                name: "IX_ItensOrcamento_ServicoId",
                table: "ItensOrcamento",
                column: "ServicoId");

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUsers_ClienteId",
                table: "AspNetUsers",
                column: "ClienteId",
                unique: true,
                filter: "\"ClienteId\" IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_AlertasEstoque_PecaId",
                table: "AlertasEstoque",
                column: "PecaId",
                unique: true,
                filter: "\"DataResolucao\" IS NULL");

            migrationBuilder.AddForeignKey(
                name: "FK_AlertasEstoque_Pecas_PecaId",
                table: "AlertasEstoque",
                column: "PecaId",
                principalTable: "Pecas",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_AspNetUsers_Clientes_ClienteId",
                table: "AspNetUsers",
                column: "ClienteId",
                principalTable: "Clientes",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_ItensOrcamento_Pecas_PecaId",
                table: "ItensOrcamento",
                column: "PecaId",
                principalTable: "Pecas",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_ItensOrcamento_Servicos_ServicoId",
                table: "ItensOrcamento",
                column: "ServicoId",
                principalTable: "Servicos",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_ItensOrdemDeServico_Pecas_PecaId",
                table: "ItensOrdemDeServico",
                column: "PecaId",
                principalTable: "Pecas",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_ItensOrdemDeServico_Servicos_ServicoId",
                table: "ItensOrdemDeServico",
                column: "ServicoId",
                principalTable: "Servicos",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Orcamentos_OrdensDeServico_OrdemDeServicoId",
                table: "Orcamentos",
                column: "OrdemDeServicoId",
                principalTable: "OrdensDeServico",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_OrdensDeServico_Clientes_ClienteId",
                table: "OrdensDeServico",
                column: "ClienteId",
                principalTable: "Clientes",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_OrdensDeServico_Veiculos_VeiculoId",
                table: "OrdensDeServico",
                column: "VeiculoId",
                principalTable: "Veiculos",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Reservas_OrdensDeServico_OrdemDeServicoId",
                table: "Reservas",
                column: "OrdemDeServicoId",
                principalTable: "OrdensDeServico",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Veiculos_Clientes_ClienteId",
                table: "Veiculos",
                column: "ClienteId",
                principalTable: "Clientes",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AlertasEstoque_Pecas_PecaId",
                table: "AlertasEstoque");

            migrationBuilder.DropForeignKey(
                name: "FK_AspNetUsers_Clientes_ClienteId",
                table: "AspNetUsers");

            migrationBuilder.DropForeignKey(
                name: "FK_ItensOrcamento_Pecas_PecaId",
                table: "ItensOrcamento");

            migrationBuilder.DropForeignKey(
                name: "FK_ItensOrcamento_Servicos_ServicoId",
                table: "ItensOrcamento");

            migrationBuilder.DropForeignKey(
                name: "FK_ItensOrdemDeServico_Pecas_PecaId",
                table: "ItensOrdemDeServico");

            migrationBuilder.DropForeignKey(
                name: "FK_ItensOrdemDeServico_Servicos_ServicoId",
                table: "ItensOrdemDeServico");

            migrationBuilder.DropForeignKey(
                name: "FK_Orcamentos_OrdensDeServico_OrdemDeServicoId",
                table: "Orcamentos");

            migrationBuilder.DropForeignKey(
                name: "FK_OrdensDeServico_Clientes_ClienteId",
                table: "OrdensDeServico");

            migrationBuilder.DropForeignKey(
                name: "FK_OrdensDeServico_Veiculos_VeiculoId",
                table: "OrdensDeServico");

            migrationBuilder.DropForeignKey(
                name: "FK_Reservas_OrdensDeServico_OrdemDeServicoId",
                table: "Reservas");

            migrationBuilder.DropForeignKey(
                name: "FK_Veiculos_Clientes_ClienteId",
                table: "Veiculos");

            migrationBuilder.DropIndex(
                name: "IX_Orcamentos_OrdemDeServicoId_Versao",
                table: "Orcamentos");

            migrationBuilder.DropIndex(
                name: "IX_ItensOrdemDeServico_PecaId",
                table: "ItensOrdemDeServico");

            migrationBuilder.DropIndex(
                name: "IX_ItensOrdemDeServico_ServicoId",
                table: "ItensOrdemDeServico");

            migrationBuilder.DropIndex(
                name: "IX_ItensOrcamento_PecaId",
                table: "ItensOrcamento");

            migrationBuilder.DropIndex(
                name: "IX_ItensOrcamento_ServicoId",
                table: "ItensOrcamento");

            migrationBuilder.DropIndex(
                name: "IX_AspNetUsers_ClienteId",
                table: "AspNetUsers");

            migrationBuilder.DropIndex(
                name: "IX_AlertasEstoque_PecaId",
                table: "AlertasEstoque");

            migrationBuilder.CreateIndex(
                name: "IX_AlertasEstoque_PecaId",
                table: "AlertasEstoque",
                column: "PecaId");
        }
    }
}
