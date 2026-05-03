using HospitalApp.Core.Application.Common;
using HospitalApp.Core.Application.Features.Billing.DTOs;
using MediatR;

namespace HospitalApp.Core.Application.Features.Billing.Commands.ProcessPayment;

public record ProcessPaymentCommand(Guid InvoiceId, ProcessPaymentRequest Request, Guid ReceivedByUserId)
    : IRequest<Result<Guid>>;
