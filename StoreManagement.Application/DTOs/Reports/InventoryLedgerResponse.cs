using System;
using System.Collections.Generic;

namespace StoreManagement.Application.DTOs.Reports;

public class InventoryLedgerResponse
{
    public int ProductId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public int StartQuantity { get; set; }
    public int EndQuantity { get; set; }
    public List<LedgerMovementResponse> Movements { get; set; } = new List<LedgerMovementResponse>();
}