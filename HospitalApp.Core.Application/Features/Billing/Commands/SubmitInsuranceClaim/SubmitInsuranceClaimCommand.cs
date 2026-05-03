using HospitalApp.Core.Application.Common;
using MediatR;

namespace HospitalApp.Core.Application.Features.Billing.Commands.SubmitInsuranceClaim;

public record SubmitInsuranceClaimCommand(Guid InvoiceId, string ClaimReferenceNumber, Guid SubmittedByUserId) : IRequest<Result>;
