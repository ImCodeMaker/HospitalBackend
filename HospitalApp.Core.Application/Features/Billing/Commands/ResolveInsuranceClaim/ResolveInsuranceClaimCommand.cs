using HospitalApp.Core.Application.Common;
using MediatR;

namespace HospitalApp.Core.Application.Features.Billing.Commands.ResolveInsuranceClaim;

public record ResolveInsuranceClaimCommand(Guid InvoiceId, bool Approved, decimal? ApprovedAmount, string? Notes, Guid ResolvedByUserId) : IRequest<Result>;
