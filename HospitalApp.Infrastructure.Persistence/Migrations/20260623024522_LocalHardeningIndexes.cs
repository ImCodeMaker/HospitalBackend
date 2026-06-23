using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HospitalApp.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class LocalHardeningIndexes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_invoices_PatientId",
                table: "invoices");

            migrationBuilder.CreateIndex(
                name: "IX_patients_Email",
                table: "patients",
                column: "Email");

            migrationBuilder.CreateIndex(
                name: "IX_patients_LastName",
                table: "patients",
                column: "LastName");

            migrationBuilder.CreateIndex(
                name: "IX_patients_Phone",
                table: "patients",
                column: "Phone");

            migrationBuilder.CreateIndex(
                name: "IX_invoices_DueDate",
                table: "invoices",
                column: "DueDate");

            migrationBuilder.CreateIndex(
                name: "IX_invoices_PatientId_Status_CreatedAt",
                table: "invoices",
                columns: new[] { "PatientId", "Status", "CreatedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_invoices_Status_CreatedAt",
                table: "invoices",
                columns: new[] { "Status", "CreatedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_consults_DoctorId_Status_CreatedAt",
                table: "consults",
                columns: new[] { "DoctorId", "Status", "CreatedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_consults_PatientId_CreatedAt",
                table: "consults",
                columns: new[] { "PatientId", "CreatedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_appointments_AssignedDoctorId_Status_ScheduledDate",
                table: "appointments",
                columns: new[] { "AssignedDoctorId", "Status", "ScheduledDate" });

            migrationBuilder.CreateIndex(
                name: "IX_appointments_Status_ScheduledDate",
                table: "appointments",
                columns: new[] { "Status", "ScheduledDate" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_patients_Email",
                table: "patients");

            migrationBuilder.DropIndex(
                name: "IX_patients_LastName",
                table: "patients");

            migrationBuilder.DropIndex(
                name: "IX_patients_Phone",
                table: "patients");

            migrationBuilder.DropIndex(
                name: "IX_invoices_DueDate",
                table: "invoices");

            migrationBuilder.DropIndex(
                name: "IX_invoices_PatientId_Status_CreatedAt",
                table: "invoices");

            migrationBuilder.DropIndex(
                name: "IX_invoices_Status_CreatedAt",
                table: "invoices");

            migrationBuilder.DropIndex(
                name: "IX_consults_DoctorId_Status_CreatedAt",
                table: "consults");

            migrationBuilder.DropIndex(
                name: "IX_consults_PatientId_CreatedAt",
                table: "consults");

            migrationBuilder.DropIndex(
                name: "IX_appointments_AssignedDoctorId_Status_ScheduledDate",
                table: "appointments");

            migrationBuilder.DropIndex(
                name: "IX_appointments_Status_ScheduledDate",
                table: "appointments");

            migrationBuilder.CreateIndex(
                name: "IX_invoices_PatientId",
                table: "invoices",
                column: "PatientId");
        }
    }
}
