using HospitalApp.Core.Application.Common;
using HospitalApp.Core.Domain.Enums;
using MediatR;

namespace HospitalApp.Core.Application.Features.Medications.Commands.AdjustStock;

public record AdjustStockCommand(
    Guid MedicationId,
    int Quantity,
    StockTransactionTypeEnum Type,
    string? Reason,
    Guid PerformedByUserId
) : IRequest<Result<int>>;
