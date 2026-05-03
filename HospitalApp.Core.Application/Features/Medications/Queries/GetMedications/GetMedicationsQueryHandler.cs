using AutoMapper;
using HospitalApp.Core.Application.Common;
using HospitalApp.Core.Application.Features.Medications.DTOs;
using HospitalApp.Core.Domain.Interfaces;
using MediatR;

namespace HospitalApp.Core.Application.Features.Medications.Queries.GetMedications;

public class GetMedicationsQueryHandler(IUnitOfWork uow, IMapper mapper)
    : IRequestHandler<GetMedicationsQuery, Result<PaginatedResult<MedicationDto>>>
{
    public async Task<Result<PaginatedResult<MedicationDto>>> Handle(GetMedicationsQuery query, CancellationToken ct)
    {
        var all = await uow.Medications.FindAsync(m =>
            m.IsActive &&
            (string.IsNullOrEmpty(query.SearchTerm) ||
             m.GenericName.Contains(query.SearchTerm) ||
             (m.BrandName != null && m.BrandName.Contains(query.SearchTerm))) &&
            (!query.LowStockOnly.HasValue || !query.LowStockOnly.Value || m.IsLowStock) &&
            (!query.OutOfStockOnly.HasValue || !query.OutOfStockOnly.Value || m.IsOutOfStock), ct);

        var total = all.Count;
        var items = all.Skip((query.PageNumber - 1) * query.PageSize).Take(query.PageSize).ToList();

        return Result<PaginatedResult<MedicationDto>>.Success(
            PaginatedResult<MedicationDto>.Create(mapper.Map<IReadOnlyList<MedicationDto>>(items), total, query.PageNumber, query.PageSize));
    }
}
