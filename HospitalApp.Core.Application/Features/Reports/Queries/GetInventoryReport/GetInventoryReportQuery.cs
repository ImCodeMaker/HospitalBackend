using HospitalApp.Core.Application.Common;
using HospitalApp.Core.Application.Features.Reports.DTOs;
using MediatR;

namespace HospitalApp.Core.Application.Features.Reports.Queries.GetInventoryReport;

public record GetInventoryReportQuery(bool LowStockOnly = false) : IRequest<Result<List<InventoryReportItemDto>>>;
