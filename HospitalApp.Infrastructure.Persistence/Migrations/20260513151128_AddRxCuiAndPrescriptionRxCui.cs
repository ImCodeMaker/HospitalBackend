using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HospitalApp.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddRxCuiAndPrescriptionRxCui : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "RxCui",
                table: "medications",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "RxCui",
                table: "MedicalPrescriptions",
                type: "text",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "RxCui",
                table: "medications");

            migrationBuilder.DropColumn(
                name: "RxCui",
                table: "MedicalPrescriptions");
        }
    }
}
