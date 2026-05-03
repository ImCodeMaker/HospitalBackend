using HospitalApp.Core.Application.Common;
using HospitalApp.Core.Application.Features.Reports.DTOs;
using HospitalApp.Core.Domain.Enums;
using HospitalApp.Core.Domain.Interfaces;
using MediatR;

namespace HospitalApp.Core.Application.Features.Reports.Queries.GetAccountsReceivable;

public class GetAccountsReceivableQueryHandler(IUnitOfWork uow)
    : IRequestHandler<GetAccountsReceivableQuery, Result<List<AccountsReceivableDto>>>
{
    public async Task<Result<List<AccountsReceivableDto>>> Handle(GetAccountsReceivableQuery query, CancellationToken ct)
    {
        var unpaidInvoices = await uow.Invoices.FindAsync(
            i => i.Status == InvoiceStatusEnum.AwaitingPayment
                 || i.Status == InvoiceStatusEnum.PartiallyPaid, ct);

        var today = DateTime.UtcNow.Date;
        var result = unpaidInvoices.Select(i =>
        {
            var days = (today - i.CreatedAt.Date).Days;
            var bucket = days switch
            {
                <= 30 => "0-30",
                <= 60 => "31-60",
                <= 90 => "61-90",
                _ => "90+"
            };
            var patientName = i.Patient != null
                ? $"{i.Patient.FirstName} {i.Patient.LastName}"
                : "—";
            return new AccountsReceivableDto(
                i.Id, i.InvoiceNumber, patientName,
                i.BalanceDue, i.CreatedAt, days, bucket);
        }).OrderByDescending(r => r.DaysOutstanding).ToList();

        return Result<List<AccountsReceivableDto>>.Success(result);
    }
}
