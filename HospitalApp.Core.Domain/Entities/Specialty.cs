using HospitalApp.Core.Domain.Enums;

namespace HospitalApp.Core.Domain.Entities;

public class Specialty : SharedEntity
{
    public required string Name { get; set; }
    public required string Code { get; set; }
    public required SpecialtyTypeEnum Type { get; set; }
    public string? Description { get; set; }
    public int DefaultConsultDurationMinutes { get; set; } = 30;

    public ICollection<Consult> Consults { get; set; } = [];
}
