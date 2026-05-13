using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HospitalApp.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddNcfAndInsuranceDenial : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "InsuranceDenialReason",
                table: "invoices",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Ncf",
                table: "invoices",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "NcfType",
                table: "invoices",
                type: "integer",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "NcfSequences",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Type = table.Column<int>(type: "integer", nullable: false),
                    CurrentSequence = table.Column<long>(type: "bigint", nullable: false),
                    MaxSequence = table.Column<long>(type: "bigint", nullable: false),
                    ExpirationDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_NcfSequences", x => x.Id);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "NcfSequences");

            migrationBuilder.DropColumn(
                name: "InsuranceDenialReason",
                table: "invoices");

            migrationBuilder.DropColumn(
                name: "Ncf",
                table: "invoices");

            migrationBuilder.DropColumn(
                name: "NcfType",
                table: "invoices");
        }
    }
}
