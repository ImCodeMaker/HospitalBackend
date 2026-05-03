using HospitalApp.Core.Application.Common;
using HospitalApp.Core.Application.Features.Specialties.DTOs;
using MediatR;

namespace HospitalApp.Core.Application.Features.Specialties.Queries.GetSpecialties;

public record GetSpecialtiesQuery : IRequest<Result<List<SpecialtyDto>>>;
