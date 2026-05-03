using HospitalApp.Core.Application.Common;
using HospitalApp.Core.Application.Features.Reports.DTOs;
using MediatR;

namespace HospitalApp.Core.Application.Features.Reports.Queries.GetDailyRevenue;

public record GetDailyRevenueQuery(DateTime Date) : IRequest<Result<DailyRevenueSummaryDto>>;
