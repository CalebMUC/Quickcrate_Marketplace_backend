# PostgreSQL Execution Strategy Guide

## Issue Fixed
**Error**: `The configured execution strategy 'NpgsqlRetryingExecutionStrategy' does not support user-initiated transactions.`

**Root Cause**: When using PostgreSQL with Entity Framework and retry execution strategies enabled, you cannot directly use `BeginTransactionAsync()` inside methods. You must wrap the transaction logic with the execution strategy.

## Solution Pattern

### ? **Incorrect Pattern** (Causes Error)
```csharp
public async Task<SomeResult> SomeMethod()
{
    using var transaction = await _dbContext.Database.BeginTransactionAsync();
    try
    {
        // Your database operations
        await _dbContext.SaveChangesAsync();
        await transaction.CommitAsync();
        return result;
    }
    catch (Exception ex)
    {
        await transaction.RollbackAsync();
        throw;
    }
}
```

### ? **Correct Pattern** (Fixed)
```csharp
public async Task<SomeResult> SomeMethod()
{
    var strategy = _dbContext.Database.CreateExecutionStrategy();

    return await strategy.ExecuteAsync(async () =>
    {
        using var transaction = await _dbContext.Database.BeginTransactionAsync();
        try
        {
            // Your database operations
            await _dbContext.SaveChangesAsync();
            await transaction.CommitAsync();
            return result;
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            throw;
        }
    });
}
```

## Methods Fixed in Your Codebase

### 1. RegisterMerchantAsync ? Fixed
- **File**: `Repositories/Merchant/MerchantRepo.cs`
- **Change**: Wrapped transaction logic with `CreateExecutionStrategy()`
- **Status**: Now properly handles retries and transactions

### 2. ApproveMerchantAsync ? Already Correct
- **File**: `Repositories/Merchant/MerchantRepo.cs`  
- **Status**: Already uses the correct pattern
- **No Change Needed**: This method was implemented correctly

## Why This Happens

PostgreSQL's `NpgsqlRetryingExecutionStrategy` provides automatic retry logic for transient failures (network issues, connection drops, etc.). However, when you manually start a transaction, the retry strategy can't safely retry operations because:

1. **Transaction State**: Manual transactions have state that can't be easily reset
2. **Side Effects**: Operations within transactions might have already executed partially
3. **Deadlock Recovery**: The strategy needs full control to handle deadlocks and retries

## Best Practices

### 1. **Always Use Execution Strategy for Transactions**
```csharp
var strategy = _dbContext.Database.CreateExecutionStrategy();
return await strategy.ExecuteAsync(async () =>
{
    using var transaction = await _dbContext.Database.BeginTransactionAsync();
    // ... transaction logic
});
```

### 2. **Simple Operations Don't Need Manual Transactions**
```csharp
// This is fine - no manual transaction needed
public async Task<SomeEntity> UpdateSimpleEntity(SomeEntity entity)
{
    _dbContext.SomeEntities.Update(entity);
    await _dbContext.SaveChangesAsync();
    return entity;
}
```

### 3. **Use Transactions Only When Necessary**
- Multiple related operations that must succeed together
- Cross-table operations that need atomicity
- Complex business logic requiring rollback capability

## When You Might Encounter This Error

### Common Scenarios:
1. **Merchant Registration**: Multiple tables (Merchants, MerchantPaymentMethods, Users)
2. **Order Processing**: Orders, OrderItems, Inventory updates
3. **User Registration**: Users, Profiles, Initial settings
4. **Bulk Operations**: Multiple related inserts/updates

### Simple Fix Checklist:
1. ? Look for `BeginTransactionAsync()` calls
2. ? Wrap the entire method logic with `CreateExecutionStrategy().ExecuteAsync()`
3. ? Keep the transaction logic inside the Execute block
4. ? Test with retry scenarios (network issues, etc.)

## Configuration (Optional)

If you want to disable retry strategies entirely (not recommended for production):

```csharp
// In Program.cs or Startup.cs
services.AddDbContext<MinimartDBContext>(options =>
{
    options.UseNpgsql(connectionString, npgsqlOptions =>
    {
        npgsqlOptions.EnableRetryOnFailure(0); // Disable retries
    });
});
```

## Testing the Fix

### Test Cases:
1. **Normal Operations**: Ensure registration works normally
2. **Network Issues**: Simulate connection drops during registration
3. **Concurrent Operations**: Multiple registrations simultaneously
4. **Payment Method Failures**: Invalid payment method IDs

### Sample Test:
```csharp
[Fact]
public async Task RegisterMerchant_ShouldHandleTransactionCorrectly()
{
    // Arrange
    var dto = new MerchantRegistrationDto { /* test data */ };
    
    // Act
    var result = await _merchantRepo.RegisterMerchantAsync(dto);
    
    // Assert
    Assert.NotNull(result);
    Assert.NotEqual(Guid.Empty, result.Id);
}
```

## Summary

The execution strategy pattern ensures your database operations are resilient to transient failures while maintaining ACID transaction properties. This fix makes your merchant registration process more robust in production environments with potential network instability or database connection issues.