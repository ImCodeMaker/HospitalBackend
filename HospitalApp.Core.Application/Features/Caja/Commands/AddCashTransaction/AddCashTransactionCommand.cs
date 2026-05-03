using HospitalApp.Core.Application.Common;
using HospitalApp.Core.Domain.Enums;
using MediatR;

namespace HospitalApp.Core.Application.Features.Caja.Commands.AddCashTransaction;

public record AddCashTransactionCommand(
    Guid ShiftId,
    CashTransactionTypeEnum Type,
    decimal Amount,
    string? Description,
    Guid? InvoiceId,
    Guid CreatedByUserId
) : IRequest<Result<Guid>>;
