using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AuditFlow.Consumer.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class Initial_Migration : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "DataAuditTransactions",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    EventId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    IdentityUserId = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    EventDateUtc = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DataAuditTransactions", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "DataAuditTransactionDetails",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    DataAuditTransactionId = table.Column<long>(type: "bigint", nullable: false),
                    DataAuditTransactionType = table.Column<int>(type: "int", nullable: false),
                    EntityName = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    PrimaryKeyValue = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    PropertyName = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    OriginalValue = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    NewValue = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DataAuditTransactionDetails", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DataAuditTransactionDetails_DataAuditTransactions_DataAuditTransactionId",
                        column: x => x.DataAuditTransactionId,
                        principalTable: "DataAuditTransactions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_DataAuditTransactionDetails_DataAuditTransactionId",
                table: "DataAuditTransactionDetails",
                column: "DataAuditTransactionId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "DataAuditTransactionDetails");

            migrationBuilder.DropTable(
                name: "DataAuditTransactions");
        }
    }
}
