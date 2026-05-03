using HospitalApp.Core.Application.Common;
using HospitalApp.Core.Application.Features.Reports.DTOs;
using MediatR;

namespace HospitalApp.Core.Application.Features.Reports.Queries.GetAccountsReceivable;

public record GetAccountsReceivableQuery : IRequest<Result<List<AccountsReceivableDto>>>;
