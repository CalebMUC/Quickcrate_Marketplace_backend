using Microsoft.EntityFrameworkCore;
using Minimart_Api.Data;
using Minimart_Api.DTOS.Payouts;
using Minimart_Api.Models;

namespace Minimart_Api.Services.Payouts
{
    /// <summary>
    /// Core business logic implementation for payouts
    /// </summary>
    public class PayoutService : IPayoutService
    {
        private readonly MinimartDBContext _context;
        private readonly ILogger<PayoutService> _logger;

        // Default commission rate (5%)
        private const decimal DefaultCommissionRate = 0.05m;

        // Valid status transitions
        private readonly Dictionary<string, List<string>> _validStatusTransitions = new()
        {
            [PayoutStatus.Pending] = new() { PayoutStatus.Scheduled, PayoutStatus.Cancelled },
            [PayoutStatus.Scheduled] = new() { PayoutStatus.Processing, PayoutStatus.Cancelled },
            [PayoutStatus.Processing] = new() { PayoutStatus.Completed, PayoutStatus.Failed },
            [PayoutStatus.Failed] = new() { PayoutStatus.Scheduled, PayoutStatus.Cancelled },
            [PayoutStatus.Completed] = new() { }, // Final state
            [PayoutStatus.Cancelled] = new() { } // Final state
        };

        public PayoutService(MinimartDBContext context, ILogger<PayoutService> logger)
        {
            _context = context;
            _logger = logger;
        }

        #region Merchant Payout Methods

        public async Task<PayoutStatsDto> GetMerchantPayoutStatsAsync(Guid merchantId)
        {
            try
            {
                var payouts = await _context.Payouts
                    .Where(p => p.MerchantId == merchantId)
                    .ToListAsync();

                var stats = new PayoutStatsDto
                {
                    TotalEarnings = payouts.Sum(p => p.GrossAmount),
                    PendingAmount = payouts.Where(p => p.Status != PayoutStatus.Completed && p.Status != PayoutStatus.Cancelled)
                                          .Sum(p => p.NetAmount),
                    CompletedAmount = payouts.Where(p => p.Status == PayoutStatus.Completed)
                                            .Sum(p => p.NetAmount),
                    TotalCommissionPaid = payouts.Sum(p => p.CommissionAmount),
                    TotalPayouts = payouts.Count,
                    PendingPayouts = payouts.Count(p => p.Status != PayoutStatus.Completed && p.Status != PayoutStatus.Cancelled),
                    CompletedPayouts = payouts.Count(p => p.Status == PayoutStatus.Completed),
                    AveragePayoutAmount = payouts.Any() ? payouts.Average(p => p.NetAmount) : 0,
                    LastPayoutDate = payouts.Where(p => p.Status == PayoutStatus.Completed)
                                           .OrderByDescending(p => p.CompletedDate)
                                           .FirstOrDefault()?.CompletedDate,
                    NextScheduledPayoutDate = payouts.Where(p => p.Status == PayoutStatus.Scheduled)
                                                   .OrderBy(p => p.ScheduledDate)
                                                   .FirstOrDefault()?.ScheduledDate
                };

                return stats;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting payout stats for merchant {MerchantId}", merchantId);
                throw;
            }
        }

        public async Task<PagedResult<PayoutDto>> GetMerchantPayoutsAsync(
            Guid merchantId,
            string? status = null,
            DateTime? startDate = null,
            DateTime? endDate = null,
            int page = 1,
            int pageSize = 20)
        {
            try
            {
                var query = _context.Payouts
                    .Include(p => p.Merchant)
                    .Include(p => p.PaymentMethod)
                    .Where(p => p.MerchantId == merchantId);

                // Apply filters
                if (!string.IsNullOrEmpty(status))
                    query = query.Where(p => p.Status == status);

                if (startDate.HasValue)
                    query = query.Where(p => p.PeriodStartDate >= startDate.Value);

                if (endDate.HasValue)
                    query = query.Where(p => p.PeriodEndDate <= endDate.Value);

                var totalCount = await query.CountAsync();

                var payouts = await query
                    .OrderByDescending(p => p.CreatedDate)
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .Select(p => new PayoutDto
                    {
                        PayoutId = p.PayoutId,
                        MerchantId = p.MerchantId,
                        MerchantName = p.Merchant.MerchantName ?? "",
                        GrossAmount = p.GrossAmount,
                        CommissionAmount = p.CommissionAmount,
                        NetAmount = p.NetAmount,
                        CommissionRate = p.CommissionRate,
                        Status = p.Status,
                        PeriodStartDate = p.PeriodStartDate,
                        PeriodEndDate = p.PeriodEndDate,
                        CreatedDate = p.CreatedDate,
                        ScheduledDate = p.ScheduledDate,
                        CompletedDate = p.CompletedDate,
                        OrderCount = p.OrderCount,
                        ProductCount = p.ProductCount,
                        PaymentMethodName = p.PaymentMethod != null ? p.PaymentMethod.Name : null,
                        Notes = p.Notes,
                        FailureReason = p.FailureReason
                    })
                    .ToListAsync();

                return new PagedResult<PayoutDto>
                {
                    Data = payouts,
                    TotalCount = totalCount,
                    Page = page,
                    PageSize = pageSize
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting merchant payouts for {MerchantId}", merchantId);
                throw;
            }
        }

        public async Task<PayoutDetailDto?> GetPayoutByIdAsync(Guid payoutId, Guid merchantId)
        {
            try
            {
                var payout = await _context.Payouts
                    .Include(p => p.Merchant)
                    .Include(p => p.PaymentMethod)
                    .Include(p => p.PayoutTransactions)
                    .Where(p => p.PayoutId == payoutId && p.MerchantId == merchantId)
                    .FirstOrDefaultAsync();

                if (payout == null)
                    return null;

                return MapToPayoutDetailDto(payout);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting payout {PayoutId} for merchant {MerchantId}", payoutId, merchantId);
                throw;
            }
        }

        public async Task<PagedResult<PayoutTransactionDto>> GetMerchantTransactionsAsync(
            Guid merchantId,
            DateTime? startDate = null,
            DateTime? endDate = null,
            string? payoutStatus = null,
            int page = 1,
            int pageSize = 20)
        {
            try
            {
                var query = _context.PayoutTransactions
                    .Include(pt => pt.Payout)
                    .Where(pt => pt.Payout.MerchantId == merchantId);

                // Apply filters
                if (startDate.HasValue)
                    query = query.Where(pt => pt.OrderCompletedDate >= startDate.Value);

                if (endDate.HasValue)
                    query = query.Where(pt => pt.OrderCompletedDate <= endDate.Value);

                if (!string.IsNullOrEmpty(payoutStatus))
                    query = query.Where(pt => pt.Payout.Status == payoutStatus);

                var totalCount = await query.CountAsync();

                var transactions = await query
                    .OrderByDescending(pt => pt.CreatedDate)
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .Select(pt => new PayoutTransactionDto
                    {
                        PayoutTransactionId = pt.PayoutTransactionId,
                        PayoutId = pt.PayoutId,
                        OrderId = pt.OrderId,
                        OrderAmount = pt.OrderAmount,
                        CommissionAmount = pt.CommissionAmount,
                        NetAmount = pt.NetAmount,
                        CommissionRate = pt.CommissionRate,
                        OrderCompletedDate = pt.OrderCompletedDate,
                        CreatedDate = pt.CreatedDate,
                        CustomerName = pt.CustomerName,
                        OrderStatus = pt.OrderStatus,
                        ItemCount = pt.ItemCount
                    })
                    .ToListAsync();

                return new PagedResult<PayoutTransactionDto>
                {
                    Data = transactions,
                    TotalCount = totalCount,
                    Page = page,
                    PageSize = pageSize
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting merchant transactions for {MerchantId}", merchantId);
                throw;
            }
        }

        #endregion

        #region Admin Payout Methods

        public async Task<PagedResult<PayoutDto>> GetAllPayoutsAsync(PayoutFilters filters)
        {
            try
            {
                var query = _context.Payouts
                    .Include(p => p.Merchant)
                    .Include(p => p.PaymentMethod)
                    .AsQueryable();

                // Apply filters
                if (filters.MerchantId.HasValue)
                    query = query.Where(p => p.MerchantId == filters.MerchantId.Value);

                if (!string.IsNullOrEmpty(filters.Status))
                    query = query.Where(p => p.Status == filters.Status);

                if (filters.StartDate.HasValue)
                    query = query.Where(p => p.PeriodStartDate >= filters.StartDate.Value);

                if (filters.EndDate.HasValue)
                    query = query.Where(p => p.PeriodEndDate <= filters.EndDate.Value);

                if (filters.MinAmount.HasValue)
                    query = query.Where(p => p.NetAmount >= filters.MinAmount.Value);

                if (filters.MaxAmount.HasValue)
                    query = query.Where(p => p.NetAmount <= filters.MaxAmount.Value);

                // Apply sorting
                query = filters.SortBy.ToLower() switch
                {
                    "grossamount" => filters.SortOrder.ToLower() == "desc" 
                        ? query.OrderByDescending(p => p.GrossAmount)
                        : query.OrderBy(p => p.GrossAmount),
                    "netamount" => filters.SortOrder.ToLower() == "desc" 
                        ? query.OrderByDescending(p => p.NetAmount)
                        : query.OrderBy(p => p.NetAmount),
                    "status" => filters.SortOrder.ToLower() == "desc" 
                        ? query.OrderByDescending(p => p.Status)
                        : query.OrderBy(p => p.Status),
                    _ => filters.SortOrder.ToLower() == "desc" 
                        ? query.OrderByDescending(p => p.CreatedDate)
                        : query.OrderBy(p => p.CreatedDate)
                };

                var totalCount = await query.CountAsync();

                var payouts = await query
                    .Skip((filters.Page - 1) * filters.PageSize)
                    .Take(filters.PageSize)
                    .Select(p => new PayoutDto
                    {
                        PayoutId = p.PayoutId,
                        MerchantId = p.MerchantId,
                        MerchantName = p.Merchant.BusinessName ?? p.Merchant.MerchantName ?? "",
                        GrossAmount = p.GrossAmount,
                        CommissionAmount = p.CommissionAmount,
                        NetAmount = p.NetAmount,
                        CommissionRate = p.CommissionRate,
                        Status = p.Status,
                        PeriodStartDate = p.PeriodStartDate,
                        PeriodEndDate = p.PeriodEndDate,
                        CreatedDate = p.CreatedDate,
                        ScheduledDate = p.ScheduledDate,
                        CompletedDate = p.CompletedDate,
                        OrderCount = p.OrderCount,
                        ProductCount = p.ProductCount,
                        PaymentMethodName = p.PaymentMethod != null ? p.PaymentMethod.Name : null,
                        Notes = p.Notes,
                        FailureReason = p.FailureReason
                    })
                    .ToListAsync();

                return new PagedResult<PayoutDto>
                {
                    Data = payouts,
                    TotalCount = totalCount,
                    Page = filters.Page,
                    PageSize = filters.PageSize
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting all payouts with filters");
                throw;
            }
        }

        public async Task<PayoutDetailDto?> GetPayoutByIdAsync(Guid payoutId)
        {
            try
            {
                var payout = await _context.Payouts
                    .Include(p => p.Merchant)
                    .Include(p => p.PaymentMethod)
                    .Include(p => p.PayoutTransactions)
                    .Where(p => p.PayoutId == payoutId)
                    .FirstOrDefaultAsync();

                if (payout == null)
                    return null;

                return MapToPayoutDetailDto(payout);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting payout {PayoutId}", payoutId);
                throw;
            }
        }

        public async Task<GeneratePayoutsResponse> GenerateWeeklyPayoutsAsync(GeneratePayoutsRequest request)
        {
            var strategy = _context.Database.CreateExecutionStrategy();

            return await strategy.ExecuteAsync(async () =>
            {
                using var transaction = await _context.Database.BeginTransactionAsync();
                try
                {
                    _logger.LogInformation("Starting payout generation for period {Start} - {End}", 
                        request.PeriodStartDate, request.PeriodEndDate);

                    var response = new GeneratePayoutsResponse { Success = false };

                    // Get eligible orders
                    var eligibleOrders = await GetEligibleOrdersForPayoutAsync(
                        request.PeriodStartDate, 
                        request.PeriodEndDate, 
                        request.MerchantIds);

                    if (!eligibleOrders.Any())
                    {
                        response.Message = "No eligible orders found for the specified period.";
                        response.Success = true;
                        return response;
                    }

                    // Group orders by merchant
                    var ordersByMerchant = eligibleOrders
                        .SelectMany(o => o.OrderProducts.Select(op => new { Order = o, MerchantId = op.MerchantID }))
                        .GroupBy(x => x.MerchantId)
                        .ToList();

                    var commissionRate = request.CommissionRate ?? DefaultCommissionRate;
                    var generatedPayouts = new List<PayoutDto>();

                    foreach (var merchantGroup in ordersByMerchant)
                    {
                        var merchantId = merchantGroup.Key;
                        var merchantOrders = merchantGroup.Select(x => x.Order).Distinct().ToList();

                        // Check if merchant has valid payment method
                        if (!await MerchantHasValidPaymentMethodAsync(merchantId))
                        {
                            response.Warnings.Add($"Merchant {merchantId} does not have a valid payment method. Skipping payout generation.");
                            continue;
                        }

                        // Calculate totals for this merchant
                        var grossAmount = merchantOrders.Sum(o => o.TotalPaymentAmount);
                        var commissionAmount = CalculateCommission(grossAmount, commissionRate);
                        var netAmount = grossAmount - commissionAmount;

                        // Get unique product count
                        var productCount = merchantOrders
                            .SelectMany(o => o.OrderProducts.Where(op => op.MerchantID == merchantId))
                            .Select(op => op.ProductId)
                            .Distinct()
                            .Count();

                        // Create payout
                        var payout = new Payout
                        {
                            PayoutId = Guid.NewGuid(),
                            MerchantId = merchantId,
                            GrossAmount = grossAmount,
                            CommissionAmount = commissionAmount,
                            CommissionRate = commissionRate,
                            NetAmount = netAmount,
                            Status = request.ProcessImmediately ? PayoutStatus.Processing : PayoutStatus.Pending,
                            PeriodStartDate = request.PeriodStartDate,
                            PeriodEndDate = request.PeriodEndDate,
                            CreatedDate = DateTime.UtcNow,
                            ScheduledDate = request.ProcessImmediately ? DateTime.UtcNow : null,
                            OrderCount = merchantOrders.Count,
                            ProductCount = productCount,
                            Notes = request.Notes,
                            CreatedBy = "System"
                        };

                        // Get merchant's primary payment method
                        var primaryPaymentMethod = await _context.MerchantPaymentMethods
                            .Where(mpm => mpm.MerchantId == merchantId && mpm.IsEnabled)
                            .FirstOrDefaultAsync();

                        if (primaryPaymentMethod != null)
                        {
                            payout.PaymentMethodId = primaryPaymentMethod.PaymentMethodId;
                        }

                        _context.Payouts.Add(payout);
                        await _context.SaveChangesAsync();

                        // Create payout transactions
                        foreach (var order in merchantOrders)
                        {
                            var orderCommission = CalculateCommission(order.TotalPaymentAmount, commissionRate);
                            var orderNet = order.TotalPaymentAmount - orderCommission;

                            var payoutTransaction = new PayoutTransaction
                            {
                                PayoutTransactionId = Guid.NewGuid(),
                                PayoutId = payout.PayoutId,
                                OrderId = order.OrderID,
                                OrderAmount = order.TotalPaymentAmount,
                                CommissionAmount = orderCommission,
                                NetAmount = orderNet,
                                CommissionRate = commissionRate,
                                OrderCompletedDate = order.OrderDate,
                                CreatedDate = DateTime.UtcNow,
                                CustomerName = order.OrderedBy,
                                OrderStatus = order.Status,
                                ItemCount = order.OrderProducts.Count(op => op.MerchantID == merchantId)
                            };

                            _context.PayoutTransactions.Add(payoutTransaction);
                        }

                        await _context.SaveChangesAsync();

                        // Add to response
                        generatedPayouts.Add(new PayoutDto
                        {
                            PayoutId = payout.PayoutId,
                            MerchantId = payout.MerchantId,
                            GrossAmount = payout.GrossAmount,
                            CommissionAmount = payout.CommissionAmount,
                            NetAmount = payout.NetAmount,
                            Status = payout.Status,
                            OrderCount = payout.OrderCount,
                            ProductCount = payout.ProductCount
                        });

                        _logger.LogInformation("Generated payout {PayoutId} for merchant {MerchantId} - Net Amount: {NetAmount}", 
                            payout.PayoutId, merchantId, netAmount);
                    }

                    await transaction.CommitAsync();

                    // Prepare response
                    response.Success = true;
                    response.Message = $"Successfully generated {generatedPayouts.Count} payouts.";
                    response.PayoutsGenerated = generatedPayouts.Count;
                    response.TotalGrossAmount = generatedPayouts.Sum(p => p.GrossAmount);
                    response.TotalCommissionAmount = generatedPayouts.Sum(p => p.CommissionAmount);
                    response.TotalNetAmount = generatedPayouts.Sum(p => p.NetAmount);
                    response.GeneratedPayouts = generatedPayouts;

                    _logger.LogInformation("Completed payout generation. Generated {Count} payouts, Total Net: {TotalNet}", 
                        generatedPayouts.Count, response.TotalNetAmount);

                    return response;
                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync();
                    _logger.LogError(ex, "Error generating payouts");
                    throw;
                }
            });
        }

        public async Task<bool> UpdatePayoutStatusAsync(Guid payoutId, UpdatePayoutStatusRequest request, string updatedBy)
        {
            try
            {
                var payout = await _context.Payouts.FindAsync(payoutId);
                if (payout == null)
                {
                    _logger.LogWarning("Payout {PayoutId} not found", payoutId);
                    return false;
                }

                // Validate status transition
                if (!IsValidStatusTransition(payout.Status, request.Status))
                {
                    _logger.LogWarning("Invalid status transition from {CurrentStatus} to {NewStatus} for payout {PayoutId}", 
                        payout.Status, request.Status, payoutId);
                    return false;
                }

                // Update payout
                payout.Status = request.Status;
                payout.UpdatedBy = updatedBy;
                payout.UpdatedDate = DateTime.UtcNow;

                if (!string.IsNullOrEmpty(request.Reason))
                {
                    payout.FailureReason = request.Reason;
                }

                if (!string.IsNullOrEmpty(request.ExternalPaymentReference))
                {
                    payout.ExternalPaymentReference = request.ExternalPaymentReference;
                }

                if (!string.IsNullOrEmpty(request.Notes))
                {
                    payout.Notes = string.IsNullOrEmpty(payout.Notes) 
                        ? request.Notes 
                        : $"{payout.Notes}\n{DateTime.UtcNow:yyyy-MM-dd HH:mm}: {request.Notes}";
                }

                // Set completion date if completed
                if (request.Status == PayoutStatus.Completed)
                {
                    payout.CompletedDate = DateTime.UtcNow;
                }

                await _context.SaveChangesAsync();

                _logger.LogInformation("Updated payout {PayoutId} status to {Status} by {UpdatedBy}", 
                    payoutId, request.Status, updatedBy);

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating payout status for {PayoutId}", payoutId);
                throw;
            }
        }

        public async Task<PagedResult<MerchantPayoutSummaryDto>> GetMerchantPayoutSummariesAsync(
            int page = 1,
            int pageSize = 20,
            string? searchTerm = null)
        {
            try
            {
                var query = _context.Merchants.AsQueryable();

                if (!string.IsNullOrEmpty(searchTerm))
                {
                    var search = searchTerm.ToLower();
                    query = query.Where(m => 
                        m.BusinessName!.ToLower().Contains(search) ||
                        m.MerchantName!.ToLower().Contains(search) ||
                        m.Email!.ToLower().Contains(search));
                }

                var totalCount = await query.CountAsync();

                var merchantSummaries = await query
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .Select(m => new MerchantPayoutSummaryDto
                    {
                        MerchantId = m.MerchantID,
                        MerchantName = m.MerchantName ?? "",
                        BusinessName = m.BusinessName ?? "",
                        TotalEarnings = m.Products
                            .SelectMany(p => p.OrderProducts)
                            .Where(op => op.Order.Status == "Completed" || op.Order.Status == "Delivered")
                            .Sum(op => op.TotalPrice),
                        TotalCommissionPaid = _context.Payouts
                            .Where(p => p.MerchantId == m.MerchantID)
                            .Sum(p => p.CommissionAmount),
                        TotalPayouts = _context.Payouts.Count(p => p.MerchantId == m.MerchantID),
                        PendingPayouts = _context.Payouts.Count(p => p.MerchantId == m.MerchantID && 
                            p.Status != PayoutStatus.Completed && p.Status != PayoutStatus.Cancelled),
                        LastPayoutDate = _context.Payouts
                            .Where(p => p.MerchantId == m.MerchantID && p.Status == PayoutStatus.Completed)
                            .OrderByDescending(p => p.CompletedDate)
                            .Select(p => p.CompletedDate)
                            .FirstOrDefault(),
                        Status = m.Status ?? "Active"
                    })
                    .ToListAsync();

                return new PagedResult<MerchantPayoutSummaryDto>
                {
                    Data = merchantSummaries,
                    TotalCount = totalCount,
                    Page = page,
                    PageSize = pageSize
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting merchant payout summaries");
                throw;
            }
        }

        #endregion

        #region Utility Methods

        public decimal CalculateCommission(decimal orderAmount, decimal commissionRate)
        {
            return Math.Round(orderAmount * commissionRate, 2);
        }

        public bool IsValidStatusTransition(string currentStatus, string newStatus)
        {
            if (_validStatusTransitions.TryGetValue(currentStatus, out var validNextStates))
            {
                return validNextStates.Contains(newStatus);
            }
            return false;
        }

        public async Task<List<Minimart_Api.Models.Order>> GetEligibleOrdersForPayoutAsync(
            DateTime periodStart, 
            DateTime periodEnd, 
            List<Guid>? merchantIds = null)
        {
            try
            {
                var query = _context.Orders
                    .Include(o => o.OrderProducts)
                    .Where(o => o.OrderDate >= periodStart && 
                               o.OrderDate <= periodEnd &&
                               (o.Status == "Completed" || o.Status == "Delivered"));

                // Filter by specific merchants if provided
                if (merchantIds != null && merchantIds.Any())
                {
                    query = query.Where(o => o.OrderProducts.Any(op => merchantIds.Contains(op.MerchantID)));
                }

                // Exclude orders already included in payouts
                var ordersInPayouts = await _context.PayoutTransactions
                    .Select(pt => pt.OrderId)
                    .ToListAsync();

                query = query.Where(o => !ordersInPayouts.Contains(o.OrderID));

                return await query.ToListAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting eligible orders for payout");
                throw;
            }
        }

        public async Task<bool> MerchantHasValidPaymentMethodAsync(Guid merchantId)
        {
            try
            {
                return await _context.MerchantPaymentMethods
                    .AnyAsync(mpm => mpm.MerchantId == merchantId && mpm.IsEnabled);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking payment method for merchant {MerchantId}", merchantId);
                return false;
            }
        }

        #endregion

        #region Private Helper Methods

        private PayoutDetailDto MapToPayoutDetailDto(Payout payout)
        {
            return new PayoutDetailDto
            {
                PayoutId = payout.PayoutId,
                MerchantId = payout.MerchantId,
                MerchantName = payout.Merchant.BusinessName ?? payout.Merchant.MerchantName ?? "",
                GrossAmount = payout.GrossAmount,
                CommissionAmount = payout.CommissionAmount,
                NetAmount = payout.NetAmount,
                CommissionRate = payout.CommissionRate,
                Status = payout.Status,
                PeriodStartDate = payout.PeriodStartDate,
                PeriodEndDate = payout.PeriodEndDate,
                CreatedDate = payout.CreatedDate,
                ScheduledDate = payout.ScheduledDate,
                CompletedDate = payout.CompletedDate,
                OrderCount = payout.OrderCount,
                ProductCount = payout.ProductCount,
                PaymentMethodName = payout.PaymentMethod?.Name,
                Notes = payout.Notes,
                FailureReason = payout.FailureReason,
                ExternalPaymentReference = payout.ExternalPaymentReference,
                CreatedBy = payout.CreatedBy,
                UpdatedBy = payout.UpdatedBy,
                UpdatedDate = payout.UpdatedDate,
                Transactions = payout.PayoutTransactions.Select(pt => new PayoutTransactionDto
                {
                    PayoutTransactionId = pt.PayoutTransactionId,
                    PayoutId = pt.PayoutId,
                    OrderId = pt.OrderId,
                    OrderAmount = pt.OrderAmount,
                    CommissionAmount = pt.CommissionAmount,
                    NetAmount = pt.NetAmount,
                    CommissionRate = pt.CommissionRate,
                    OrderCompletedDate = pt.OrderCompletedDate,
                    CreatedDate = pt.CreatedDate,
                    CustomerName = pt.CustomerName,
                    OrderStatus = pt.OrderStatus,
                    ItemCount = pt.ItemCount
                }).ToList()
            };
        }

        #endregion
    }
}