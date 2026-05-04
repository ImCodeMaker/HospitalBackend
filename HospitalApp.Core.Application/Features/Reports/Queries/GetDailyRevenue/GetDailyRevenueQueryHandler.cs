using HospitalApp.Core.Application.Common;
using HospitalApp.Core.Application.Features.Reports.DTOs;
using HospitalApp.Core.Domain.Enums;
using HospitalApp.Core.Domain.Interfaces;
using MediatR;

namespace HospitalApp.Core.Application.Features.Reports.Queries.GetDailyRevenue;

public class GetDailyRevenueQueryHandler(IUnitOfWork uow)
    : IRequestHandler<GetDailyRevenueQuery, Result<DailyRevenueSummaryDto>>
{
    public async Task<Result<DailyRevenueSummaryDto>> Handle(GetDailyRevenueQuery query, CancellationToken ct)
    {
        var day = DateTime.SpecifyKind(query.Date.Date, DateTimeKind.Utc);
        var next = day.AddDays(1);

        var payments = await uow.Payments.FindAsync(
            p => p.PaymentDate >= day && p.PaymentDate < next, ct);

        var invoices = await uow.Invoices.FindAsync(
            i => i.CreatedAt >= day && i.CreatedAt < next, ct);

        var cashRev = payments.Where(p => p.Method == PaymentMethodEnum.Cash).Sum(p => p.Amount);
        var cardRev = payments.Where(p => p.Method is PaymentMethodEnum.CreditCard or PaymentMethodEnum.DebitCard).Sum(p => p.Amount);
        var transferRev = payments.Where(p => p.Method == PaymentMethodEnum.BankTransfer).Sum(p => p.Amount);
        var insuranceRev = payments.Where(p => p.Method == PaymentMethodEnum.Insurance).Sum(p => p.Amount);

        return Result<DailyRevenueSummaryDto>.Success(new DailyRevenueSummaryDto(
            day,
            cashRev + cardRev + transferRev + insuranceRev,
            cashRev, cardRev, transferRev, insuranceRev,
            invoices.Count,
            invoices.Count(i => i.Status == InvoiceStatusEnum.Paid)
        ));
    }
}
