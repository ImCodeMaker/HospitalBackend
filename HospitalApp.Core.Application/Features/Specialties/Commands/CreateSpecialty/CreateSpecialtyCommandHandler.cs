using HospitalApp.Core.Application.Common;
using HospitalApp.Core.Domain.Entities;
using HospitalApp.Core.Domain.Interfaces;
using MediatR;

namespace HospitalApp.Core.Application.Features.Specialties.Commands.CreateSpecialty;

public class CreateSpecialtyCommandHandler(IUnitOfWork uow)
    : IRequestHandler<CreateSpecialtyCommand, Result<Guid>>
{
    public async Task<Result<Guid>> Handle(CreateSpecialtyCommand command, CancellationToken ct)
    {
        var req = command.Request;
        if (await uow.Specialties.ExistsAsync(s => s.Code == req.Code, ct))
            return Result<Guid>.Failure($"Specialty with code '{req.Code}' already exists.", 409);

        var specialty = new Specialty
        {
            Name = req.Name,
            Code = req.Code,
            Type = req.Type,
            Description = req.Description,
            DefaultConsultDurationMinutes = req.DefaultConsultDurationMinutes > 0 ? req.DefaultConsultDurationMinutes : 30,
        };

        await uow.Specialties.AddAsync(specialty, ct);
        await uow.SaveChangesAsync(ct);
        return Result<Guid>.Created(specialty.Id);
    }
}
