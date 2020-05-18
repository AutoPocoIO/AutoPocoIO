using Microsoft.EntityFrameworkCore.Migrations;
using System;

namespace AutoPocoIO.Migrations
{
    internal partial class LogDb : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "AutoPocoLog");

            migrationBuilder.CreateTable(
                name: "Request",
                schema: "AutoPocoLog",
                columns: table => new
                {
                    RequestId = table.Column<long>(nullable: false),
                    RequestGuid = table.Column<Guid>(nullable: false),
                    RequesterIp = table.Column<string>(maxLength: 39, nullable: true),
                    DateTimeUtc = table.Column<DateTime>(type: "datetime2(4)", nullable: true),
                    DayOfRequest = table.Column<DateTime>(nullable: true, computedColumnSql: "CONVERT(date, DateTimeUtc)"),
                    RequestType = table.Column<string>(maxLength: 10, nullable: true),
                    Connector = table.Column<string>(maxLength: 50, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Request", x => new { x.RequestId, x.RequestGuid });
                });

            migrationBuilder.CreateTable(
                name: "Response",
                schema: "AutoPocoLog",
                columns: table => new
                {
                    ResponseId = table.Column<long>(nullable: false),
                    RequestGuid = table.Column<Guid>(nullable: false),
                    DateTimeUtc = table.Column<DateTime>(type: "datetime2(4)", nullable: true),
                    DayOfResponse = table.Column<DateTime>(nullable: true, computedColumnSql: "CONVERT(date, DateTimeUtc)"),
                    Status = table.Column<string>(maxLength: 51, nullable: true),
                    Exception = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Response", x => new { x.ResponseId, x.RequestGuid });
                });

            migrationBuilder.CreateIndex(
                name: "IX_Request_DateTimeUtc",
                schema: "AutoPocoLog",
                table: "Request",
                column: "DateTimeUtc");

            migrationBuilder.CreateIndex(
                name: "IX_DayWithIP",
                schema: "AutoPocoLog",
                table: "Request",
                column: "DayOfRequest")
                .Annotation("SqlServer:Include", new[] { "RequesterIp" });

            migrationBuilder.CreateIndex(
                name: "IX_Request_RequestGuid",
                schema: "AutoPocoLog",
                table: "Request",
                column: "RequestGuid")
                .Annotation("SqlServer:Include", new[] { "DateTimeUtc", "RequestType", "RequesterIp", "Connector" });

            migrationBuilder.CreateIndex(
                name: "IX_DayAndType",
                schema: "AutoPocoLog",
                table: "Request",
                columns: new[] { "DayOfRequest", "RequestType" });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Request",
                schema: "AutoPocoLog");

            migrationBuilder.DropTable(
                name: "Response",
                schema: "AutoPocoLog");
        }
    }
}
