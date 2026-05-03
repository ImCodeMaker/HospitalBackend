namespace HospitalApp.Core.Application.Features.Specialties.DTOs;

public class SpecialtyDto
{
    public Guid Id { get; init; }
    public string Name { get; init; } = string.Empty;
    public string Code { get; init; } = string.Empty;
    public string Type { get; init; } = string.Empty;
    public string? Description { get; init; }
    public int DefaultConsultDurationMinutes { get; init; }
}
