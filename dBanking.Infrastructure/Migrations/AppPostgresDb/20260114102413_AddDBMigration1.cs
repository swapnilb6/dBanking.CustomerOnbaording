using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace dBanking.Infrastructure.Migrations.AppPostgresDb
{
    /// <inheritdoc />
    public partial class AddDBMigration1 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "public");

            migrationBuilder.RenameTable(
                name: "AuditRecords",
                newName: "AuditRecords",
                newSchema: "public");

            migrationBuilder.RenameColumn(
                name: "AuditId",
                schema: "public",
                table: "AuditRecords",
                newName: "AuditRecordId");

            migrationBuilder.AlterColumn<string>(
                name: "EntityType",
                schema: "public",
                table: "AuditRecords",
                type: "character varying(100)",
                maxLength: 100,
                nullable: false,
                oldClrType: typeof(int),
                oldType: "integer");

            migrationBuilder.AlterColumn<string>(
                name: "CorrelationId",
                schema: "public",
                table: "AuditRecords",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Action",
                schema: "public",
                table: "AuditRecords",
                type: "character varying(100)",
                maxLength: 100,
                nullable: false,
                oldClrType: typeof(int),
                oldType: "integer");

            migrationBuilder.AddColumn<string>(
                name: "Environment",
                schema: "public",
                table: "AuditRecords",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "RelatedEntityId",
                schema: "public",
                table: "AuditRecords",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Source",
                schema: "public",
                table: "AuditRecords",
                type: "text",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "InboxState",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    MessageId = table.Column<Guid>(type: "uuid", nullable: false),
                    ConsumerId = table.Column<Guid>(type: "uuid", nullable: false),
                    LockId = table.Column<Guid>(type: "uuid", nullable: false),
                    RowVersion = table.Column<byte[]>(type: "bytea", rowVersion: true, nullable: true),
                    Received = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ReceiveCount = table.Column<int>(type: "integer", nullable: false),
                    ExpirationTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    Consumed = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    Delivered = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    LastSequenceNumber = table.Column<long>(type: "bigint", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_InboxState", x => x.Id);
                    table.UniqueConstraint("AK_InboxState_MessageId_ConsumerId", x => new { x.MessageId, x.ConsumerId });
                });

            migrationBuilder.CreateTable(
                name: "OutboxMessage",
                columns: table => new
                {
                    SequenceNumber = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    EnqueueTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    SentTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Headers = table.Column<string>(type: "text", nullable: true),
                    Properties = table.Column<string>(type: "text", nullable: true),
                    InboxMessageId = table.Column<Guid>(type: "uuid", nullable: true),
                    InboxConsumerId = table.Column<Guid>(type: "uuid", nullable: true),
                    OutboxId = table.Column<Guid>(type: "uuid", nullable: true),
                    MessageId = table.Column<Guid>(type: "uuid", nullable: false),
                    ContentType = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    Body = table.Column<string>(type: "text", nullable: false),
                    ConversationId = table.Column<Guid>(type: "uuid", nullable: true),
                    CorrelationId = table.Column<Guid>(type: "uuid", nullable: true),
                    InitiatorId = table.Column<Guid>(type: "uuid", nullable: true),
                    RequestId = table.Column<Guid>(type: "uuid", nullable: true),
                    SourceAddress = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    DestinationAddress = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    ResponseAddress = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    FaultAddress = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    ExpirationTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OutboxMessage", x => x.SequenceNumber);
                });

            migrationBuilder.CreateTable(
                name: "OutboxState",
                columns: table => new
                {
                    OutboxId = table.Column<Guid>(type: "uuid", nullable: false),
                    LockId = table.Column<Guid>(type: "uuid", nullable: false),
                    RowVersion = table.Column<byte[]>(type: "bytea", rowVersion: true, nullable: true),
                    Created = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Delivered = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    LastSequenceNumber = table.Column<long>(type: "bigint", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OutboxState", x => x.OutboxId);
                });

            migrationBuilder.UpdateData(
                schema: "public",
                table: "AuditRecords",
                keyColumn: "AuditRecordId",
                keyValue: new Guid("aaaabbbb-cccc-dddd-eeee-ffff00000001"),
                columns: new[] { "Action", "EntityType", "Environment", "RelatedEntityId", "Source", "Timestamp" },
                values: new object[] { "CREATE", "Customer", null, null, null, new DateTimeOffset(new DateTime(2025, 12, 1, 10, 36, 30, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)) });

            migrationBuilder.UpdateData(
                schema: "public",
                table: "AuditRecords",
                keyColumn: "AuditRecordId",
                keyValue: new Guid("aaaabbbb-cccc-dddd-eeee-ffff00000002"),
                columns: new[] { "Action", "EntityType", "Environment", "RelatedEntityId", "Source", "Timestamp" },
                values: new object[] { "CREATE", "KycCase", null, null, null, new DateTimeOffset(new DateTime(2025, 12, 1, 10, 36, 50, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)) });

            migrationBuilder.UpdateData(
                schema: "public",
                table: "AuditRecords",
                keyColumn: "AuditRecordId",
                keyValue: new Guid("aaaabbbb-cccc-dddd-eeee-ffff00000003"),
                columns: new[] { "Action", "EntityType", "Environment", "RelatedEntityId", "Source", "Timestamp" },
                values: new object[] { "UPDATE", "Customer", null, null, null, new DateTimeOffset(new DateTime(2025, 12, 5, 14, 0, 20, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)) });

            migrationBuilder.CreateIndex(
                name: "IX_AuditRecords_Action",
                schema: "public",
                table: "AuditRecords",
                column: "Action");

            migrationBuilder.CreateIndex(
                name: "IX_AuditRecords_CorrelationId",
                schema: "public",
                table: "AuditRecords",
                column: "CorrelationId");

            migrationBuilder.CreateIndex(
                name: "IX_AuditRecords_EntityType_TargetEntityId",
                schema: "public",
                table: "AuditRecords",
                columns: new[] { "EntityType", "TargetEntityId" });

            migrationBuilder.CreateIndex(
                name: "IX_AuditRecords_Timestamp",
                schema: "public",
                table: "AuditRecords",
                column: "Timestamp");

            migrationBuilder.CreateIndex(
                name: "IX_InboxState_Delivered",
                table: "InboxState",
                column: "Delivered");

            migrationBuilder.CreateIndex(
                name: "IX_OutboxMessage_EnqueueTime",
                table: "OutboxMessage",
                column: "EnqueueTime");

            migrationBuilder.CreateIndex(
                name: "IX_OutboxMessage_ExpirationTime",
                table: "OutboxMessage",
                column: "ExpirationTime");

            migrationBuilder.CreateIndex(
                name: "IX_OutboxMessage_InboxMessageId_InboxConsumerId_SequenceNumber",
                table: "OutboxMessage",
                columns: new[] { "InboxMessageId", "InboxConsumerId", "SequenceNumber" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_OutboxMessage_OutboxId_SequenceNumber",
                table: "OutboxMessage",
                columns: new[] { "OutboxId", "SequenceNumber" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_OutboxState_Created",
                table: "OutboxState",
                column: "Created");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "InboxState");

            migrationBuilder.DropTable(
                name: "OutboxMessage");

            migrationBuilder.DropTable(
                name: "OutboxState");

            migrationBuilder.DropIndex(
                name: "IX_AuditRecords_Action",
                schema: "public",
                table: "AuditRecords");

            migrationBuilder.DropIndex(
                name: "IX_AuditRecords_CorrelationId",
                schema: "public",
                table: "AuditRecords");

            migrationBuilder.DropIndex(
                name: "IX_AuditRecords_EntityType_TargetEntityId",
                schema: "public",
                table: "AuditRecords");

            migrationBuilder.DropIndex(
                name: "IX_AuditRecords_Timestamp",
                schema: "public",
                table: "AuditRecords");

            migrationBuilder.DropColumn(
                name: "Environment",
                schema: "public",
                table: "AuditRecords");

            migrationBuilder.DropColumn(
                name: "RelatedEntityId",
                schema: "public",
                table: "AuditRecords");

            migrationBuilder.DropColumn(
                name: "Source",
                schema: "public",
                table: "AuditRecords");

            migrationBuilder.RenameTable(
                name: "AuditRecords",
                schema: "public",
                newName: "AuditRecords");

            migrationBuilder.RenameColumn(
                name: "AuditRecordId",
                table: "AuditRecords",
                newName: "AuditId");

            migrationBuilder.AlterColumn<int>(
                name: "EntityType",
                table: "AuditRecords",
                type: "integer",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(100)",
                oldMaxLength: 100);

            migrationBuilder.AlterColumn<string>(
                name: "CorrelationId",
                table: "AuditRecords",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(100)",
                oldMaxLength: 100,
                oldNullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "Action",
                table: "AuditRecords",
                type: "integer",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(100)",
                oldMaxLength: 100);

            migrationBuilder.UpdateData(
                table: "AuditRecords",
                keyColumn: "AuditId",
                keyValue: new Guid("aaaabbbb-cccc-dddd-eeee-ffff00000001"),
                columns: new[] { "Action", "EntityType", "Timestamp" },
                values: new object[] { 0, 0, new DateTime(2025, 12, 1, 10, 36, 30, 0, DateTimeKind.Utc) });

            migrationBuilder.UpdateData(
                table: "AuditRecords",
                keyColumn: "AuditId",
                keyValue: new Guid("aaaabbbb-cccc-dddd-eeee-ffff00000002"),
                columns: new[] { "Action", "EntityType", "Timestamp" },
                values: new object[] { 0, 1, new DateTime(2025, 12, 1, 10, 36, 50, 0, DateTimeKind.Utc) });

            migrationBuilder.UpdateData(
                table: "AuditRecords",
                keyColumn: "AuditId",
                keyValue: new Guid("aaaabbbb-cccc-dddd-eeee-ffff00000003"),
                columns: new[] { "Action", "EntityType", "Timestamp" },
                values: new object[] { 1, 0, new DateTime(2025, 12, 5, 14, 0, 20, 0, DateTimeKind.Utc) });
        }
    }
}
