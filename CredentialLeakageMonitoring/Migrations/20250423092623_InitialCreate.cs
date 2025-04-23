using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CredentialLeakageMonitoring.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Customers",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Customers", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Leaks",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    EmailHash = table.Column<byte[]>(type: "bytea", maxLength: 64, nullable: false),
                    EMailAlgorithm = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    ObfuscatedPassword = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    PasswordHash = table.Column<byte[]>(type: "bytea", maxLength: 64, nullable: false),
                    PasswordSalt = table.Column<byte[]>(type: "bytea", maxLength: 16, nullable: false),
                    PasswordAlgorithmVersion = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    PasswordAlgorithm = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    Domain = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    FirstSeen = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    LastSeen = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    CustomerId = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Leaks", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Leaks_Customers_CustomerId",
                        column: x => x.CustomerId,
                        principalTable: "Customers",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_Leaks_CustomerId",
                table: "Leaks",
                column: "CustomerId");

            migrationBuilder.CreateIndex(
                name: "IX_Leaks_Domain",
                table: "Leaks",
                column: "Domain");

            migrationBuilder.CreateIndex(
                name: "IX_Leaks_EmailHash",
                table: "Leaks",
                column: "EmailHash");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Leaks");

            migrationBuilder.DropTable(
                name: "Customers");
        }
    }
}
