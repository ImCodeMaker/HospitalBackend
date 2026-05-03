using HospitalApp.Core.Application.Common;
using HospitalApp.Core.Application.Features.Billing.DTOs;
using MediatR;

namespace HospitalApp.Core.Application.Features.Billing.Commands.CreateInvoice;

public record CreateInvoiceCommand(CreateInvoiceRequest Request, Guid CreatedByUserId) : IRequest<Result<Guid>>;
