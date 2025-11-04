using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StoreManagement.API.Models;
using StoreManagement.Application.Services;
using System.Collections.Generic;
using StoreManagement.Application.DTOs.Reports;

namespace StoreManagement.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Policy = "AdminOrStaff")]
public class ReportsController : ControllerBase
{
    private readonly IReportService _reportService;
    private readonly ILogger<ReportsController> _logger;

    public ReportsController(IReportService reportService, ILogger<ReportsController> logger)
    {
        _reportService = reportService;
        _logger = logger;
    }

    [HttpGet("sales/overview")]
    public async Task<IActionResult> GetSalesOverview(
        [FromQuery] DateTime startDate,
        [FromQuery] DateTime endDate,
        [FromQuery] string groupBy = "day")
    {
        if (startDate > endDate)
        {
            return BadRequest(ApiResponse.ErrorResponse("startDate cannot be after endDate."));
        }

        try
        {
            var salesData = await _reportService.GetSalesOverviewAsync(startDate, endDate, groupBy);
            return Ok(ApiResponse<IEnumerable<object>>.SuccessResponse(salesData, "Sales overview retrieved successfully."));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while generating sales overview report.");
            return StatusCode(500, ApiResponse.ErrorResponse("An error occurred while generating the report."));
        }
    }

    [HttpGet("products/dead-stock")]
    public async Task<IActionResult> GetDeadStockProducts(
        [FromQuery] DateTime startDate,
        [FromQuery] DateTime endDate)
    {
        if (startDate > endDate)
        {
            return BadRequest(ApiResponse.ErrorResponse("startDate cannot be after endDate."));
        }

        try
        {
            var deadStockData = await _reportService.GetDeadStockProductsAsync(startDate, endDate);
            return Ok(ApiResponse<IEnumerable<object>>.SuccessResponse(deadStockData, "Dead stock report retrieved successfully."));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while generating dead stock report.");
            return StatusCode(500, ApiResponse.ErrorResponse("An error occurred while generating the report."));
        }
    }

    [HttpGet("inventory/ledger")]
    public async Task<IActionResult> GetInventoryLedger(
        [FromQuery] int productId,
        [FromQuery] DateTime startDate,
        [FromQuery] DateTime endDate)
    {
        if (startDate > endDate)
        {
            return BadRequest(ApiResponse.ErrorResponse("startDate cannot be after endDate."));
        }

        try
        {
            var ledgerData = await _reportService.GetInventoryLedgerAsync(productId, startDate, endDate);
            return Ok(ApiResponse<object>.SuccessResponse(ledgerData, "Inventory ledger retrieved successfully."));
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Failed to get inventory ledger: {Message}", ex.Message);
            return BadRequest(ApiResponse.ErrorResponse(ex.Message));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while generating inventory ledger report.");
            return StatusCode(500, ApiResponse.ErrorResponse("An error occurred while generating the report."));
        }
    }
    
    [HttpGet("purchases/summary")]
    public async Task<IActionResult> GetPurchaseSummary(
        [FromQuery] DateTime startDate,
        [FromQuery] DateTime endDate,
        [FromQuery] string groupBy = "day")
    {
        if (startDate > endDate)
        {
            return BadRequest(ApiResponse.ErrorResponse("startDate cannot be after endDate."));
        }

        try
        {
            var summaryData = await _reportService.GetPurchaseSummaryAsync(startDate, endDate, groupBy);
            return Ok(ApiResponse<IEnumerable<PurchaseSummaryResponse>>.SuccessResponse(summaryData, "Purchase summary retrieved successfully."));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while generating purchase summary report.");
            return StatusCode(500, ApiResponse.ErrorResponse("An error occurred while generating the report."));
        }
    }
}