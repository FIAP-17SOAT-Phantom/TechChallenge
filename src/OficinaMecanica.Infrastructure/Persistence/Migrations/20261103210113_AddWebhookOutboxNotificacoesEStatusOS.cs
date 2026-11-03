using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace OficinaMecanica.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddWebhookOutboxNotificacoesEStatusOS : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "DataUltimaAtualizacao",
                table: "OrdensDeServico",
                type: "timestamp with time zone",
                nullable: false,
                defaultValueSql: "CURRENT_TIMESTAMP");

            migrationBuilder.CreateTable(
                name: "EventosRespostaOrcamento",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ExternalEventId = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    OrcamentoId = table.Column<Guid>(type: "uuid", nullable: false),
                    Resposta = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    TimestampUnix = table.Column<long>(type: "bigint", nullable: false),
                    RecebidoEmUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ProcessadoEmUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ProcessadoComSucesso = table.Column<bool>(type: "boolean", nullable: false),
                    TipoErro = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: true),
                    Erro = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EventosRespostaOrcamento", x => x.Id);
                    table.ForeignKey(
                        name: "FK_EventosRespostaOrcamento_Orcamentos_OrcamentoId",
                        column: x => x.OrcamentoId,
                        principalTable: "Orcamentos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "OutboxMessages",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Type = table.Column<string>(type: "character varying(250)", maxLength: 250, nullable: false),
                    Payload = table.Column<string>(type: "jsonb", nullable: false),
                    OccurredOnUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ProcessedOnUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    NextAttemptOnUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DeadLetterOnUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    Attempts = table.Column<int>(type: "integer", nullable: false),
                    LockedBy = table.Column<Guid>(type: "uuid", nullable: true),
                    LockedUntilUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    LastError = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OutboxMessages", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_EventosRespostaOrcamento_ExternalEventId",
                table: "EventosRespostaOrcamento",
                column: "ExternalEventId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_EventosRespostaOrcamento_OrcamentoId",
                table: "EventosRespostaOrcamento",
                column: "OrcamentoId");

            migrationBuilder.CreateIndex(
                name: "IX_EventosRespostaOrcamento_RecebidoEmUtc",
                table: "EventosRespostaOrcamento",
                column: "RecebidoEmUtc");

            migrationBuilder.CreateIndex(
                name: "IX_OutboxMessages_LockedUntilUtc",
                table: "OutboxMessages",
                column: "LockedUntilUtc");

            migrationBuilder.CreateIndex(
                name: "IX_OutboxMessages_ProcessedOnUtc_DeadLetterOnUtc_NextAttemptOn~",
                table: "OutboxMessages",
                columns: new[] { "ProcessedOnUtc", "DeadLetterOnUtc", "NextAttemptOnUtc", "Attempts" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "EventosRespostaOrcamento");

            migrationBuilder.DropTable(
                name: "OutboxMessages");

            migrationBuilder.DropColumn(
                name: "DataUltimaAtualizacao",
                table: "OrdensDeServico");
        }
    }
}
