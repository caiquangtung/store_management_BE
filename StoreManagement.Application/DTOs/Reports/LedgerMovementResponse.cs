using System;

namespace StoreManagement.Application.DTOs.Reports;

public class LedgerMovementResponse
{
    public DateTime Date { get; set; }
    public string Type { get; set; } = string.Empty;
    public string Reference { get; set; } = string.Empty;
    public int Change { get; set; } // Số lượng thay đổi (âm hoặc dương)
    public int Balance { get; set; } // Số dư sau giao dịch
}