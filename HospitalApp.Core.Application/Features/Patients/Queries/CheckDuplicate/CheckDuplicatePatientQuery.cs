using HospitalApp.Core.Application.Common;
using HospitalApp.Core.Domain.Enums;
using MediatR;

namespace HospitalApp.Core.Application.Features.Patients.Queries.CheckDuplicate;

public record CheckDuplicatePatientQuery(DocumentTypeEnum DocumentType, string DocumentNumber)
    : IRequest<Result<DuplicatePatientResult>>;

public record DuplicatePatientResult(bool IsDuplicate, Guid? ExistingPatientId, string? ExistingPatientName);
