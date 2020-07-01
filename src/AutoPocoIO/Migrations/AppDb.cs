using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

namespace AutoPocoIO.Migrations
{
    internal partial class AppDb : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "AutoPoco");

            migrationBuilder.CreateTable(
                name: "Connector",
                schema: "AutoPoco",
                columns: table => new
                {
                    Id = table.Column<string>(nullable: false),
                    Name = table.Column<string>(maxLength: 50, nullable: true),
                    ResourceType = table.Column<int>(nullable: false),
                    Schema = table.Column<string>(maxLength: 50, nullable: true),
                    ConnectionString = table.Column<string>(nullable: true),
                    RecordLimit = table.Column<int>(nullable: false),
                    UserId = table.Column<string>(maxLength: 50, nullable: true),
                    InitialCatalog = table.Column<string>(maxLength: 50, nullable: true),
                    DataSource = table.Column<string>(maxLength: 500, nullable: true),
                    Port = table.Column<int>(nullable: true),
                    IsActive = table.Column<bool>(nullable: false),
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Connector", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "UserJoin",
                schema: "AutoPoco",
                columns: table => new
                {
                    Id = table.Column<string>(nullable: false),
                    Alias = table.Column<string>(maxLength: 50, nullable: false),
                    PKConnectorId = table.Column<string>(nullable: true),
                    FKConnectorId = table.Column<string>(nullable: true),
                    PKTableName = table.Column<string>(maxLength: 100, nullable: false),
                    FKTableName = table.Column<string>(maxLength: 100, nullable: false),
                    PKColumn = table.Column<string>(maxLength: 500, nullable: false),
                    FKColumn = table.Column<string>(maxLength: 500, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserJoin", x => x.Id);
                    table.ForeignKey(
                        name: "FK_UserJoin_Connector_FKConnectorId",
                        column: x => x.FKConnectorId,
                        principalSchema: "AutoPoco",
                        principalTable: "Connector",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_UserJoin_Connector_PKConnectorId",
                        column: x => x.PKConnectorId,
                        principalSchema: "AutoPoco",
                        principalTable: "Connector",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.InsertData(
                schema: "AutoPoco",
                table: "Connector",
                columns: new[] { "Id", "ConnectionString", "DataSource", "InitialCatalog", "Name", "RecordLimit", "ResourceType", "Schema", "UserId", "IsActive" },
                values: new object[] { "4b6b6ba7-0209-4b89-91cb-0e2a67aa37c1", "", null, null, "AppDb", 500, 1, "AutoPoco", null, true });

            migrationBuilder.InsertData(
                schema: "AutoPoco",
                table: "Connector",
                columns: new[] { "Id", "ConnectionString", "DataSource", "InitialCatalog", "Name", "RecordLimit", "ResourceType", "Schema", "UserId", "IsActive" },
                values: new object[] { "4d74e770-54e9-4b0f-8f13-59ccb0808654", "", null, null, "LogDb", 500, 1, "AutoPocoLog", null, true });

            migrationBuilder.InsertData(
                schema: "AutoPoco",
                table: "UserJoin",
                columns: new[] { "Id", "Alias", "FKColumn", "FKConnectorId", "FKTableName", "PKColumn", "PKConnectorId", "PKTableName" },
                values: new object[] { "abd7f037-cc34-44fb-89f5-2e4a06772a01", "Response", "ResponseId,RequestGuid", "4d74e770-54e9-4b0f-8f13-59ccb0808654", "Response", "RequestId,RequestGuid", "4d74e770-54e9-4b0f-8f13-59ccb0808654", "Request" });

            migrationBuilder.CreateIndex(
                name: "IDX_ConnectorName",
                schema: "AutoPoco",
                table: "Connector",
                column: "Name",
                unique: true,
                filter: "[Name] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_UserJoin_Alias",
                schema: "AutoPoco",
                table: "UserJoin",
                column: "Alias",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_UserJoin_FKConnectorId",
                schema: "AutoPoco",
                table: "UserJoin",
                column: "FKConnectorId");

            migrationBuilder.CreateIndex(
                name: "IX_UserJoin_PKConnectorId",
                schema: "AutoPoco",
                table: "UserJoin",
                column: "PKConnectorId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "UserJoin",
                schema: "AutoPoco");

            migrationBuilder.DropTable(
                name: "Connector",
                schema: "AutoPoco");
        }
    }
}
