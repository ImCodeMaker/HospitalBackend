using HospitalApp.Core.Application.Common;
using HospitalApp.Core.Application.Features.Medications.DTOs;
using MediatR;

namespace HospitalApp.Core.Application.Features.Medications.Queries.GetMedications;

public record GetMedicationsQuery(
    string? SearchTerm,
    bool? LowStockOnly,
    bool? OutOfStockOnly,
    int PageNumber = 1,
    int PageSize = 50
) : IRequest<Result<PaginatedResult<MedicationDto>>>;
