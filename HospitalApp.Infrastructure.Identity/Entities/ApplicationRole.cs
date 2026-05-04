using Microsoft.AspNetCore.Identity;

namespace HospitalApp.Infrastructure.Identity.Entities;

public class ApplicationRole : IdentityRole<Guid>
{
    public string? Description { get; set; }

    public static class Roles
    {
        public const string Admin = "Admin";
        public const string Doctor = "Doctor";
        public const string Receptionist = "Receptionist";
        public const string LabTechnician = "LabTechnician";
        public const string Nurse = "Nurse";
        public const string PatientPortal = "PatientPortal";
    }
}
