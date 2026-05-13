using Asp.Versioning;
using HospitalApp.Core.Domain.Entities;
using HospitalApp.Core.Domain.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HospitalApp.WebAPI.Controllers.v1;

[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/[controller]")]
[Authorize(Policy = "AdminOnly")]
public class VendorsController(IUnitOfWork uow) : BaseController
{
    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] bool? activeOnly, CancellationToken ct)
    {
        var vendors = await uow.Vendors.FindAsync(v => activeOnly == null || v.IsActive == activeOnly, ct);
        return Ok(vendors.OrderBy(v => v.Name));
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateVendorRequest body, CancellationToken ct)
    {
        var v = new Vendor
        {
            Name = body.Name,
            Rnc = body.Rnc,
            ContactEmail = body.ContactEmail,
            ContactPhone = body.ContactPhone,
            Address = body.Address,
            Notes = body.Notes,
        };
        await uow.Vendors.AddAsync(v, ct);
        await uow.SaveChangesAsync(ct);
        return CreatedAtAction(nameof(GetAll), new { id = v.Id }, v);
    }

    [HttpGet("{vendorId:guid}/purchase-orders")]
    public async Task<IActionResult> GetPurchaseOrders(Guid vendorId, CancellationToken ct)
    {
        var pos = await uow.PurchaseOrders.FindAsync(p => p.VendorId == vendorId, ct);
        return Ok(pos.OrderByDescending(p => p.OrderDate));
    }

    [HttpPost("{vendorId:guid}/purchase-orders")]
    public async Task<IActionResult> CreatePurchaseOrder(Guid vendorId, [FromBody] CreatePurchaseOrderRequest body, CancellationToken ct)
    {
        var po = new PurchaseOrder
        {
            VendorId = vendorId,
            OrderNumber = $"PO-{DateTime.UtcNow:yyyyMMdd}-{Guid.NewGuid().ToString()[..6].ToUpper()}",
            ExpectedDeliveryDate = body.ExpectedDeliveryDate,
            Status = "Open",
            Subtotal = body.Subtotal,
            TaxAmount = body.TaxAmount,
            TotalAmount = body.Subtotal + body.TaxAmount,
            Notes = body.Notes,
        };
        await uow.PurchaseOrders.AddAsync(po, ct);
        await uow.SaveChangesAsync(ct);
        return CreatedAtAction(nameof(GetPurchaseOrders), new { vendorId }, po);
    }

    [HttpPost("{vendorId:guid}/payments")]
    public async Task<IActionResult> RecordPayment(Guid vendorId, [FromBody] SupplierPaymentRequest body, CancellationToken ct)
    {
        var pay = new SupplierPayment
        {
            VendorId = vendorId,
            PurchaseOrderId = body.PurchaseOrderId,
            PaidByUserId = GetCurrentUserId(),
            Amount = body.Amount,
            Method = body.Method,
            ReferenceNumber = body.ReferenceNumber,
            Notes = body.Notes,
        };
        await uow.SupplierPayments.AddAsync(pay, ct);

        if (body.PurchaseOrderId.HasValue)
        {
            var po = await uow.PurchaseOrders.GetByIdAsync(body.PurchaseOrderId.Value, ct);
            if (po is not null)
            {
                po.PaidAmount += body.Amount;
                po.UpdatedAt = DateTime.UtcNow;
                uow.PurchaseOrders.Update(po);
            }
        }

        await uow.SaveChangesAsync(ct);
        return Created("", pay);
    }

    [HttpGet("{vendorId:guid}/payments")]
    public async Task<IActionResult> GetPayments(Guid vendorId, CancellationToken ct)
    {
        var payments = await uow.SupplierPayments.FindAsync(p => p.VendorId == vendorId, ct);
        return Ok(payments.OrderByDescending(p => p.PaymentDate));
    }
}

public record CreateVendorRequest(string Name, string? Rnc, string? ContactEmail, string? ContactPhone, string? Address, string? Notes);
public record CreatePurchaseOrderRequest(decimal Subtotal, decimal TaxAmount, DateTime? ExpectedDeliveryDate, string? Notes);
public record SupplierPaymentRequest(decimal Amount, string Method, Guid? PurchaseOrderId, string? ReferenceNumber, string? Notes);
