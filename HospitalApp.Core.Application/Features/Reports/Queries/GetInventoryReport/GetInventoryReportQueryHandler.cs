using HospitalApp.Core.Application.Common;
using HospitalApp.Core.Application.Features.Reports.DTOs;
using HospitalApp.Core.Domain.Interfaces;
using MediatR;

namespace HospitalApp.Core.Application.Features.Reports.Queries.GetInventoryReport;

public class GetInventoryReportQueryHandler(IUnitOfWork uow)
    : IRequestHandler<GetInventoryReportQuery, Result<List<InventoryReportItemDto>>>
{
    public async Task<Result<List<InventoryReportItemDto>>> Handle(GetInventoryReportQuery query, CancellationToken ct)
    {
        var medications = await uow.Medications.FindAsync(m => m.IsActive, ct);

        var result = medications
            .Where(m => !query.LowStockOnly || m.IsLowStock || m.IsOutOfStock)
            .Select(m => new InventoryReportItemDto(
                m.Id, m.GenericName, m.BrandName,
                m.CurrentStock, m.MinimumStockThreshold, m.SalePrice,
                m.IsLowStock, m.IsOutOfStock, m.IsExpiringSoon, m.IsExpired,
                m.EarliestExpirationDate))
            .OrderBy(m => m.GenericName)
            .ToList();

        return Result<List<InventoryReportItemDto>>.Success(result);
    }
}
