namespace HospitalApp.Core.Domain.Entities;

public class Vendor : SharedEntity
{
    public required string Name { get; set; }
    public string? Rnc { get; set; }
    public string? ContactEmail { get; set; }
    public string? ContactPhone { get; set; }
    public string? Address { get; set; }
    public string? Notes { get; set; }
    public bool IsActive { get; set; } = true;

    public ICollection<PurchaseOrder> PurchaseOrders { get; set; } = [];
    public ICollection<SupplierPayment> Payments { get; set; } = [];
}

public class PurchaseOrder : SharedEntity
{
    public required Guid VendorId { get; set; }
    public required string OrderNumber { get; set; }
    public DateTime OrderDate { get; set; } = DateTime.UtcNow;
    public DateTime? ExpectedDeliveryDate { get; set; }
    public DateTime? ReceivedDate { get; set; }
    public required string Status { get; set; } = "Open"; // Open / Received / Cancelled
    public decimal Subtotal { get; set; }
    public decimal TaxAmount { get; set; }
    public decimal TotalAmount { get; set; }
    public decimal PaidAmount { get; set; }
    public decimal BalanceDue => TotalAmount - PaidAmount;
    public string? Notes { get; set; }

    public Vendor? Vendor { get; set; }
    public ICollection<SupplierPayment> Payments { get; set; } = [];
}

public class SupplierPayment : SharedEntity
{
    public required Guid VendorId { get; set; }
    public Guid? PurchaseOrderId { get; set; }
    public required Guid PaidByUserId { get; set; }
    public required decimal Amount { get; set; }
    public required string Method { get; set; } // Cash / BankTransfer / Check
    public string? ReferenceNumber { get; set; }
    public DateTime PaymentDate { get; set; } = DateTime.UtcNow;
    public string? Notes { get; set; }

    public Vendor? Vendor { get; set; }
    public PurchaseOrder? PurchaseOrder { get; set; }
}
