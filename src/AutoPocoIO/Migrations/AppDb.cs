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
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
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
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    Alias = table.Column<string>(maxLength: 50, nullable: false),
                    PKConnectorId = table.Column<int>(nullable: true),
                    FKConnectorId = table.Column<int>(nullable: true),
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
                values: new object[] { 1, "", null, null, "appDb", 500, 1, "AutoPoco", null, true });

            migrationBuilder.InsertData(
                schema: "AutoPoco",
                table: "Connector",
                columns: new[] { "Id", "ConnectionString", "DataSource", "InitialCatalog", "Name", "RecordLimit", "ResourceType", "Schema", "UserId", "IsActive" },
                values: new object[] { 2, "", null, null, "logDb", 500, 1, "AutoPocoLog", null, true });

            migrationBuilder.InsertData(
                schema: "AutoPoco",
                table: "UserJoin",
                columns: new[] { "Id", "Alias", "FKColumn", "FKConnectorId", "FKTableName", "PKColumn", "PKConnectorId", "PKTableName" },
                values: new object[] { 1, "Response", "ResponseId,RequestGuid", 2, "Response", "RequestId,RequestGuid", 2, "Request" });

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
