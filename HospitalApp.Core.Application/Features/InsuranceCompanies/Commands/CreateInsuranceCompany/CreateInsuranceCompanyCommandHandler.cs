using HospitalApp.Core.Application.Common;
using HospitalApp.Core.Domain.Entities;
using HospitalApp.Core.Domain.Interfaces;
using MediatR;

namespace HospitalApp.Core.Application.Features.InsuranceCompanies.Commands.CreateInsuranceCompany;

public class CreateInsuranceCompanyCommandHandler(IUnitOfWork uow)
    : IRequestHandler<CreateInsuranceCompanyCommand, Result<Guid>>
{
    public async Task<Result<Guid>> Handle(CreateInsuranceCompanyCommand command, CancellationToken ct)
    {
        var req = command.Request;
        var company = new InsuranceCompany
        {
            Name = req.Name,
            ContactPhone = req.ContactPhone,
            ContactEmail = req.ContactEmail,
            ClaimSubmissionInstructions = req.ClaimInstructions,
            DefaultCoveragePercentage = req.DefaultCoveragePercentage,
        };
        await uow.InsuranceCompanies.AddAsync(company, ct);
        await uow.SaveChangesAsync(ct);
        return Result<Guid>.Created(company.Id);
    }
}
