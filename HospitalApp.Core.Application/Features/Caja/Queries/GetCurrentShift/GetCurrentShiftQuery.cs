using HospitalApp.Core.Application.Common;
using HospitalApp.Core.Application.Features.Caja.DTOs;
using MediatR;

namespace HospitalApp.Core.Application.Features.Caja.Queries.GetCurrentShift;

public record GetCurrentShiftQuery : IRequest<Result<CajaShiftDto?>>;
