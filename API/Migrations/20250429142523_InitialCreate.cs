using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CredentialLeakageMonitoring.API.Migrations
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
                    Name = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Customers", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Domains",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    DomainName = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Domains", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "PasswordSalt",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Salt = table.Column<byte[]>(type: "bytea", maxLength: 16, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PasswordSalt", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "CustomerDomain",
                columns: table => new
                {
                    AssociatedByCustomersId = table.Column<Guid>(type: "uuid", nullable: false),
                    AssociatedDomainsId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CustomerDomain", x => new { x.AssociatedByCustomersId, x.AssociatedDomainsId });
                    table.ForeignKey(
                        name: "FK_CustomerDomain_Customers_AssociatedByCustomersId",
                        column: x => x.AssociatedByCustomersId,
                        principalTable: "Customers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_CustomerDomain_Domains_AssociatedDomainsId",
                        column: x => x.AssociatedDomainsId,
                        principalTable: "Domains",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
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
                    PasswordAlgorithmVersion = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    PasswordAlgorithm = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    Domain = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    FirstSeen = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    LastSeen = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    PasswordSaltId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Leaks", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Leaks_PasswordSalt_PasswordSaltId",
                        column: x => x.PasswordSaltId,
                        principalTable: "PasswordSalt",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "CustomerLeak",
                columns: table => new
                {
                    AssociatedCustomersId = table.Column<Guid>(type: "uuid", nullable: false),
                    AssociatedLeaksId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CustomerLeak", x => new { x.AssociatedCustomersId, x.AssociatedLeaksId });
                    table.ForeignKey(
                        name: "FK_CustomerLeak_Customers_AssociatedCustomersId",
                        column: x => x.AssociatedCustomersId,
                        principalTable: "Customers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_CustomerLeak_Leaks_AssociatedLeaksId",
                        column: x => x.AssociatedLeaksId,
                        principalTable: "Leaks",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_CustomerDomain_AssociatedDomainsId",
                table: "CustomerDomain",
                column: "AssociatedDomainsId");

            migrationBuilder.CreateIndex(
                name: "IX_CustomerLeak_AssociatedLeaksId",
                table: "CustomerLeak",
                column: "AssociatedLeaksId");

            migrationBuilder.CreateIndex(
                name: "IX_Customers_Name",
                table: "Customers",
                column: "Name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Domains_DomainName",
                table: "Domains",
                column: "DomainName",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Leaks_Domain",
                table: "Leaks",
                column: "Domain");

            migrationBuilder.CreateIndex(
                name: "IX_Leaks_EmailHash",
                table: "Leaks",
                column: "EmailHash");

            migrationBuilder.CreateIndex(
                name: "IX_Leaks_PasswordSaltId",
                table: "Leaks",
                column: "PasswordSaltId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CustomerDomain");

            migrationBuilder.DropTable(
                name: "CustomerLeak");

            migrationBuilder.DropTable(
                name: "Domains");

            migrationBuilder.DropTable(
                name: "Customers");

            migrationBuilder.DropTable(
                name: "Leaks");

            migrationBuilder.DropTable(
                name: "PasswordSalt");
        }
    }
}
