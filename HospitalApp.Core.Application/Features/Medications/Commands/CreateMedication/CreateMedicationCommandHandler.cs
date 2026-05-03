using HospitalApp.Core.Application.Common;
using HospitalApp.Core.Domain.Entities;
using HospitalApp.Core.Domain.Enums;
using HospitalApp.Core.Domain.Interfaces;
using MediatR;

namespace HospitalApp.Core.Application.Features.Medications.Commands.CreateMedication;

public class CreateMedicationCommandHandler(IUnitOfWork uow)
    : IRequestHandler<CreateMedicationCommand, Result<Guid>>
{
    public async Task<Result<Guid>> Handle(CreateMedicationCommand command, CancellationToken ct)
    {
        var req = command.Request;

        var medication = new Medication
        {
            GenericName = req.GenericName,
            BrandName = req.BrandName,
            AtcCode = req.AtcCode,
            Presentation = req.Presentation,
            Strength = req.Strength,
            UnitOfMeasure = req.UnitOfMeasure,
            CurrentStock = req.InitialStock,
            MinimumStockThreshold = req.MinimumStockThreshold,
            ReorderQuantity = req.ReorderQuantity,
            StorageLocation = req.StorageLocation,
            RequiresRefrigeration = req.RequiresRefrigeration,
            IsControlledSubstance = req.IsControlledSubstance,
            ControlledSubstanceClass = req.ControlledSubstanceClass,
            Supplier = req.Supplier,
            CostPrice = req.CostPrice,
            SalePrice = req.SalePrice,
            EarliestExpirationDate = req.EarliestExpirationDate,
            BatchNumber = req.BatchNumber,
            Notes = req.Notes,
        };

        await uow.Medications.AddAsync(medication, ct);

        if (req.InitialStock > 0)
        {
            await uow.StockTransactions.AddAsync(new StockTransaction
            {
                MedicationId = medication.Id,
                PerformedByUserId = command.CreatedByUserId,
                Type = StockTransactionTypeEnum.PurchaseReceipt,
                Quantity = req.InitialStock,
                StockBefore = 0,
                StockAfter = req.InitialStock,
                Reason = "Initial stock on creation",
                BatchNumber = req.BatchNumber
            }, ct);
        }

        await uow.SaveChangesAsync(ct);
        return Result<Guid>.Created(medication.Id);
    }
}
