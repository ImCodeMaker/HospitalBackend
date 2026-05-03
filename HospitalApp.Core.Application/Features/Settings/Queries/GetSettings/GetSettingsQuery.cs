using HospitalApp.Core.Application.Common;
using HospitalApp.Core.Application.Features.Settings.DTOs;
using MediatR;

namespace HospitalApp.Core.Application.Features.Settings.Queries.GetSettings;

public record GetSettingsQuery : IRequest<Result<ClinicSettingsDto>>;
