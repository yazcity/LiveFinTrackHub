using FinTrackHub.Data;
using FinTrackHub.Interfaces;
using System.Security.Claims;
using FinTrackHub.Entities;
using FinTrackHub.Common;
using FinTrackHub.Services.Interfaces;
using FinTrackHub.Services;
using Microsoft.EntityFrameworkCore;
using FinTrackHub.Models.DTOs;

namespace FinTrackHub.Repositories
{
    public class TransactionRepository : ITransactionRepository
    {
        private readonly ApplicationDbContext _context;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly ICurrentUserService _currentUserService;
        public TransactionRepository(ApplicationDbContext context, IHttpContextAccessor httpContextAccessor, ICurrentUserService currentUserService)
        {
            this._context = context;
            _httpContextAccessor = httpContextAccessor;
            _currentUserService = currentUserService;
        }
        //public string GetCurrentUserId()
        //{
        //    var user = _httpContextAccessor.HttpContext?.User;
        //    return user?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        //}

        public async Task<Result<Transaction>> AddTransactionAsync(Transaction transaction)
        {
            try
            {
                var account = await _context.Accounts.FindAsync(transaction.AccountId);
                if (account == null)
                {
                    return Result<Transaction>.Failure("Account not found");
                }

                var IncomeExpenseCategory = await _context.IncomeExpenseCategories.FindAsync(transaction.CategoryId);
                if (IncomeExpenseCategory == null)
                {
                    return Result<Transaction>.Failure("Category not found");
                }

                await _context.Transactions.AddAsync(transaction);
                await _context.SaveChangesAsync();


                // Step 3: Update the account balance based on the transaction type (deposit or withdrawal)

                if (IncomeExpenseCategory.TypeId == 3)
                {
                    account.Amount += transaction.Amount;
                    // Step 4: Save the updated account balance
                    _context.Accounts.Update(account);  // Mark the account for update
                    await _context.SaveChangesAsync();
                }
                else if (IncomeExpenseCategory.TypeId == 5)
                {
                    account.Amount += -transaction.Amount;
                    // Step 4: Save the updated account balance
                    _context.Accounts.Update(account);  // Mark the account for update
                    await _context.SaveChangesAsync();
                }

                return Result<Transaction>.Success(transaction, "The transaction has been added successfully.");
            }
            catch (Exception ex)
            {
                return Result<Transaction>.Failure(ex.Message);
            }
        }
        public async Task<Result<Transaction>> GetTransactionByIdAsync(long transactionId)
        {
            try
            {
                var transaction = await _context.Transactions
                    .Where(a => a.TransactionId == transactionId)
                    .Include(a => a.IncomeExpenseCategory)
                        .ThenInclude(ag => ag.IncomeExpenseCategoryType)
                    .Include(a => a.CreatedByUser)
                    .Include(a => a.UpdatedByUser)
                    .Include(a => a.Account)
                        .ThenInclude(aa => aa.AccountGroup)
                    .FirstOrDefaultAsync();

                if (transaction == null)
                    return Result<Transaction>.Failure("Transaction not found.");

                return Result<Transaction>.Success(transaction, "Transaction retrieved successfully.");
            }
            catch (Exception ex)
            {
                // Ideally: log the exception here using ILogger
                return Result<Transaction>.Failure($"An error occurred while retrieving the transaction: {ex.Message}");
            }
        }
        public async Task<Result<IEnumerable<Transaction>>> GetAllTransactionAsync(string? userId)
        {
            try
            {
                var currentUserId = userId ?? _currentUserService.UserId;

                if (string.IsNullOrEmpty(currentUserId))
                    return Result<IEnumerable<Transaction>>.Failure("User is not authenticated.");

                var transactionList = await _context.Transactions
                    .Where(x => x.IsActive && x.CreatedById == currentUserId)
                    .Include(t => t.Account)
                        .ThenInclude(ag => ag.AccountGroup)
                    .Include(t => t.IncomeExpenseCategory)
                        .ThenInclude(ic => ic.IncomeExpenseCategoryType)
                    .Include(t => t.CreatedByUser)
                    .Include(t => t.UpdatedByUser)
                    .ToListAsync();

                if (transactionList == null || !transactionList.Any())
                    return Result<IEnumerable<Transaction>>.Failure("No transactions found for the user.");

                return Result<IEnumerable<Transaction>>.Success(transactionList, "Transactions retrieved successfully.");
            }
            catch (Exception ex)
            {
                // Ideally log: _logger.LogError(ex, "Error retrieving transactions for user {UserId}", userId);
                return Result<IEnumerable<Transaction>>.Failure($"An error occurred while fetching transactions: {ex.Message}");
            }
        }

        //public async Task<Result<Transaction>> UpdateTransactionAsync(Transaction transaction)
        //{
        //    try
        //    {
        //        var existingTransaction = await _context.Transactions.FindAsync(transaction.TransactionId);
        //        if (existingTransaction == null)
        //        {
        //            return Result<Transaction>.Failure("Transaction not found");
        //        }

        //        var account = await _context.Accounts.FindAsync(transaction.AccountId);
        //        if (account == null)
        //        {
        //            return Result<Transaction>.Failure("Account not found");
        //        }

        //        var IncomeExpenseCategory = await _context.IncomeExpenseCategories.FindAsync(transaction.CategoryId);
        //        if (IncomeExpenseCategory == null)
        //        {
        //            return Result<Transaction>.Failure("Category not found");
        //        }

        //        // Step 1: If the account has changed, create an adjustment transaction for the old account
        //        if (existingTransaction.AccountId != transaction.AccountId)
        //        {
        //            var oldAccount = await _context.Accounts.FindAsync(existingTransaction.AccountId);
        //            if (oldAccount != null)
        //            {
        //                var balanceChange = existingTransaction.Amount; // Balance change is the previous transaction's amount

        //                var balanceChangeTransaction = new Transaction
        //                {
        //                    TransactionDate = DateTime.UtcNow,
        //                    Amount = -balanceChange, // Subtract the old transaction amount from the old account
        //                    AccountId = existingTransaction.AccountId,
        //                    CategoryId = _context.IncomeExpenseCategories
        //                        .Where(x => x.IncomeExpenseCategoryType.TypeId == 2 && !x.IsDeletable)
        //                        .Select(z => z.CategoryId)
        //                        .FirstOrDefault(),
        //                    Note = "Balance adjustment after account update (old account)",
        //                    IsActive = true,
        //                    CreatedById = transaction.UpdatedById,
        //                    UpdatedById = transaction.UpdatedById
        //                };

        //                // Add the adjustment transaction for the old account
        //                var oldAccountTransactionResult = await AddTransactionAsync(balanceChangeTransaction);
        //                if (!oldAccountTransactionResult.IsSuccess)
        //                {
        //                    return Result<Transaction>.Failure(oldAccountTransactionResult.Message);
        //                }

        //                // Revert the old account's balance
        //                if (IncomeExpenseCategory.TypeId == 3)
        //                {
        //                    oldAccount.Amount -= balanceChange; // Revert income adjustment
        //                }
        //                else if (IncomeExpenseCategory.TypeId == 5)
        //                {
        //                    oldAccount.Amount += balanceChange; // Revert expense adjustment
        //                }
        //                _context.Accounts.Update(oldAccount);
        //            }

        //            // Now, apply the balance change to the new account
        //            if (IncomeExpenseCategory.TypeId == 3)
        //            {
        //                account.Amount += (transaction.Amount);
        //            }
        //            else if (IncomeExpenseCategory.TypeId == 5)
        //            {
        //                account.Amount += -(transaction.Amount);
        //            }
        //            _context.Accounts.Update(account); // Mark the new account for update
        //        }
        //        else
        //        {
        //            if (IncomeExpenseCategory.TypeId == 3)
        //            {
        //                account.Amount += (transaction.Amount - existingTransaction.Amount);
        //                // Step 4: Save the updated account balance
        //                _context.Accounts.Update(account);  // Mark the account for update
        //                await _context.SaveChangesAsync();
        //            }
        //            else if (IncomeExpenseCategory.TypeId == 5)
        //            {
        //                account.Amount += -(transaction.Amount - existingTransaction.Amount);
        //                // Step 4: Save the updated account balance
        //                _context.Accounts.Update(account);  // Mark the account for update
        //                await _context.SaveChangesAsync();
        //            }
        //        }



        //        existingTransaction.TransactionDate = transaction.TransactionDate;
        //        existingTransaction.Amount = transaction.Amount;
        //        existingTransaction.AccountId = transaction.AccountId;
        //        existingTransaction.CategoryId = transaction.CategoryId;
        //        existingTransaction.Description = transaction.Description;
        //        existingTransaction.Note = transaction.Note;
        //        existingTransaction.Attachement = transaction.Attachement;
        //        existingTransaction.IsActive = true;
        //        existingTransaction.UpdatedDate = DateTime.UtcNow;
        //        existingTransaction.UpdatedById = transaction.UpdatedById;

        //        await _context.SaveChangesAsync();

        //        return Result<Transaction>.Success("Transaction hasbeen updated successfully");
        //    }
        //    catch (Exception ex)
        //    {
        //        return Result<Transaction>.Failure(ex.Message);
        //    }
        //}

        public async Task<Result<Transaction>> UpdateTransactionAsync(Transaction transaction)
        {
            try
            {
                var existingTransaction = await _context.Transactions.FindAsync(transaction.TransactionId);
                if (existingTransaction == null)
                    return Result<Transaction>.Failure("Transaction not found.");

                var newAccount = await _context.Accounts.FindAsync(transaction.AccountId);
                if (newAccount == null)
                    return Result<Transaction>.Failure("New account not found.");

                var category = await _context.IncomeExpenseCategories.FindAsync(transaction.CategoryId);
                if (category == null)
                    return Result<Transaction>.Failure("Category not found.");

                var isIncome = category.TypeId == 3;
                var isExpense = category.TypeId == 5;

                // If account has changed
                if (existingTransaction.AccountId != transaction.AccountId)
                {
                    var oldAccount = await _context.Accounts.FindAsync(existingTransaction.AccountId);
                    if (oldAccount == null)
                        return Result<Transaction>.Failure("Old account not found.");

                    // Revert old account balance
                    if (isIncome)
                        oldAccount.Amount -= existingTransaction.Amount;
                    else if (isExpense)
                        oldAccount.Amount += existingTransaction.Amount;

                    _context.Accounts.Update(oldAccount);

                    // Apply new amount to new account
                    if (isIncome)
                        newAccount.Amount += transaction.Amount;
                    else if (isExpense)
                        newAccount.Amount -= transaction.Amount;

                    _context.Accounts.Update(newAccount);
                }
                else
                {
                    // Same account, adjust balance difference
                    var difference = transaction.Amount - existingTransaction.Amount;

                    if (isIncome)
                        newAccount.Amount += difference;
                    else if (isExpense)
                        newAccount.Amount -= difference;

                    _context.Accounts.Update(newAccount);
                }

                // Update transaction fields
                existingTransaction.TransactionDate = transaction.TransactionDate;
                existingTransaction.Amount = transaction.Amount;
                existingTransaction.AccountId = transaction.AccountId;
                existingTransaction.CategoryId = transaction.CategoryId;
                existingTransaction.Description = transaction.Description;
                existingTransaction.Note = transaction.Note;
                existingTransaction.Attachement = transaction.Attachement;
                existingTransaction.UpdatedDate = DateTime.UtcNow;
                existingTransaction.UpdatedById = transaction.UpdatedById;

                _context.Transactions.Update(existingTransaction);
                await _context.SaveChangesAsync();

                return Result<Transaction>.Success(existingTransaction, "Transaction has been updated successfully.");
            }
            catch (Exception ex)
            {
                return Result<Transaction>.Failure($"An error occurred while updating the transaction: {ex.Message}");
            }
        }


        public async Task<Result<bool>> DeleteTransactionAsync(long transactionId, string updatedById)
        {
            try
            {
                // Find the transaction with the given TransactionId that is active
                var transaction = await _context.Transactions
                    .Where(t => t.TransactionId == transactionId && t.IsActive)
                    .Include(x => x.IncomeExpenseCategory)
                    .FirstOrDefaultAsync();

                if (transaction == null)
                {
                    return Result<bool>.Failure("No active transactions found.");
                }

                // Find the associated account
                var account = await _context.Accounts.FindAsync(transaction.AccountId);
                if (account == null)
                {
                    return Result<bool>.Failure("Account not found.");
                }

                // Adjust the account balance based on transaction category type
                decimal balanceAdjustment = 0;
                if (transaction.IncomeExpenseCategory.TypeId == 3)
                {
                    balanceAdjustment = -transaction.Amount;  // Expense (debit)
                }
                else if (transaction.IncomeExpenseCategory.TypeId == 5)
                {
                    balanceAdjustment = transaction.Amount;  // Income (credit)
                }
                else
                {
                    return Result<bool>.Failure("error category not defined.");
                }
                // Update the account balance
                account.Amount += balanceAdjustment;

                // Mark the transaction as inactive and update metadata
                transaction.IsActive = false;
                transaction.UpdatedById = updatedById;
                transaction.UpdatedDate = DateTime.UtcNow;

                // Save both the account update and transaction update in a single operation
                _context.Accounts.Update(account);
                _context.Transactions.Update(transaction);

                // Save changes in one call
                await _context.SaveChangesAsync();

                return Result<bool>.Success(true, "Success! Transaction has been removed.");
            }
            catch (Exception ex)
            {
                return Result<bool>.Failure(ex.Message);
            }
        }


        //public async Task<ResponseResult> StageUpdateTransactionAsync(Transaction transaction)
        //{
        //    try
        //    {
        //        var existingTransaction = await _context.Transactions.FindAsync(transaction.TransactionId);
        //        if (existingTransaction == null)
        //        {
        //            return new ResponseResult { Succeeded = false, Errors = new[] { "Transaction not found" } };
        //        }

        //        var account = await _context.Accounts.FindAsync(transaction.AccountId);
        //        if (account == null)
        //        {
        //            return new ResponseResult { Succeeded = false, Errors = new[] { "Account not found" } };
        //        }

        //        var IncomeExpenseCategory = await _context.IncomeExpenseCategories.FindAsync(transaction.CategoryId);
        //        if (IncomeExpenseCategory == null)
        //        {
        //            return new ResponseResult { Succeeded = false, Errors = new[] { "Category not found" } };
        //        }

        //        // Step 1: If the account has changed, create an adjustment transaction for the old account
        //        if (existingTransaction.AccountId != transaction.AccountId)
        //        {
        //            var oldAccount = await _context.Accounts.FindAsync(existingTransaction.AccountId);
        //            if (oldAccount != null)
        //            {
        //                var balanceChange = existingTransaction.Amount; // Balance change is the previous transaction's amount

        //                var balanceChangeTransaction = new Transaction
        //                {
        //                    TransactionDate = DateTime.UtcNow,
        //                    Amount = -balanceChange, // Subtract the old transaction amount from the old account
        //                    AccountId = existingTransaction.AccountId,
        //                    CategoryId = _context.IncomeExpenseCategories
        //                        .Where(x => x.IncomeExpenseCategoryType.TypeId == 2 && !x.IsDeletable)
        //                        .Select(z => z.CategoryId)
        //                        .FirstOrDefault(),
        //                    Note = "Balance adjustment after account update (old account)",
        //                    IsActive = true,
        //                    CreatedById = transaction.UpdatedById,
        //                    UpdatedById = transaction.UpdatedById
        //                };

        //                // Add the adjustment transaction for the old account
        //                var oldAccountTransactionResult = await AddTransactionAsync(balanceChangeTransaction);
        //                if (!oldAccountTransactionResult.Succeeded)
        //                {
        //                    return new ResponseResult { Succeeded = false, Errors = oldAccountTransactionResult.Errors };
        //                }

        //                // Revert the old account's balance
        //                if (IncomeExpenseCategory.TypeId == 3)
        //                {
        //                    oldAccount.Amount -= balanceChange; // Revert income adjustment
        //                }
        //                else if (IncomeExpenseCategory.TypeId == 5)
        //                {
        //                    oldAccount.Amount += balanceChange; // Revert expense adjustment
        //                }
        //                _context.Accounts.Update(oldAccount);
        //            }

        //            // Now, apply the balance change to the new account
        //            if (IncomeExpenseCategory.TypeId == 3)
        //            {
        //                account.Amount += (transaction.Amount - existingTransaction.Amount); // Add difference for income
        //            }
        //            else if (IncomeExpenseCategory.TypeId == 5)
        //            {
        //                account.Amount += -(transaction.Amount - existingTransaction.Amount); // Subtract difference for expense
        //            }
        //            _context.Accounts.Update(account); // Mark the new account for update
        //        }

        //        // Step 2: Update the transaction details
        //        existingTransaction.TransactionDate = transaction.TransactionDate;
        //        existingTransaction.Amount = transaction.Amount;
        //        existingTransaction.AccountId = transaction.AccountId;
        //        existingTransaction.CategoryId = transaction.CategoryId;
        //        existingTransaction.Description = transaction.Description;
        //        existingTransaction.Note = transaction.Note;
        //        existingTransaction.Attachement = transaction.Attachement;
        //        existingTransaction.IsActive = true;
        //        existingTransaction.UpdatedDate = DateTime.UtcNow;
        //        existingTransaction.UpdatedById = transaction.UpdatedById;

        //        // Save changes to both the account and the transaction
        //        await _context.SaveChangesAsync();

        //        return new ResponseResult { Succeeded = true };
        //    }
        //    catch (Exception ex)
        //    {
        //        return new ResponseResult { Succeeded = false, Errors = new[] { ex.Message } };
        //    }
        //}



        #region Transfer
        //public async Task<Result<bool>> TransferFundsAsync(long sourceAccountId, long destinationAccountId, decimal amount, string createdById)
        //{

        //    // var info = await GetAllTransactionAsync();

        //    // Group income, expense, and transfers by AccountId
        //    //var income = info.Where(x => x.AccountId == sourceAccountId && x.IncomeExpenseCategory != null && x.IncomeExpenseCategory.TypeId == 3).Sum(x => x.Amount);
        //    //var expense = info.Where(x => x.AccountId == sourceAccountId && x.IncomeExpenseCategory != null && x.IncomeExpenseCategory.TypeId == 5).Sum(x => x.Amount);
        //    //var transfer = info.Where(x => x.AccountId == sourceAccountId && x.IncomeExpenseCategory == null).Sum(x => x.Amount);


        //    var sourceAccount = await _context.Accounts
        //                        .FirstOrDefaultAsync(a => a.AccountId == sourceAccountId && a.IsActive);



        //    var destinationAccount = await _context.Accounts
        //                        .FirstOrDefaultAsync(a => a.AccountId == destinationAccountId && a.IsActive);

        //    if (sourceAccount == null || destinationAccount == null)
        //    {
        //        return Result<bool>.Failure("Source or destination account not found.");
        //    }

        //    // Check if source account has enough balance
        //    //if ((sourceAccount.Amount + (income - expense) + transfer) < amount)
        //    //if (sourceAccount.Amount < amount)
        //    //{
        //    //    return new OperationResult { Succeeded = false, Errors = new[] { "Insufficient balance in source account." } };
        //    //}

        //    // Create debit transaction (decrease from source account)
        //    var debitTransaction = new Transaction
        //    {
        //        TransactionDate = DateTime.UtcNow,
        //        Amount = -amount, // Debit, so it's negative
        //        AccountId = sourceAccount.AccountId,
        //        CategoryId = _context.IncomeExpenseCategories.Where(x => x.IncomeExpenseCategoryType.TypeId == 6 && x.IsDeletable == false).Select(z => z.CategoryId).FirstOrDefault(), // Add appropriate category if needed
        //        Note = "Transfer to account " + destinationAccount.AccountName,
        //        IsActive = true,
        //        CreatedById = createdById,
        //        UpdatedById = createdById
        //    };

        //    // Create credit transaction (increase in destination account)
        //    var creditTransaction = new Transaction
        //    {
        //        TransactionDate = DateTime.UtcNow,
        //        Amount = amount, // Credit, so it's positive
        //        AccountId = destinationAccount.AccountId,
        //        CategoryId = _context.IncomeExpenseCategories.Where(x => x.IncomeExpenseCategoryType.TypeId == 6 && x.IsDeletable == false).Select(z => z.CategoryId).FirstOrDefault(), // Add appropriate category if needed
        //        Note = "Transfer from account " + sourceAccount.AccountName,
        //        IsActive = true,
        //        CreatedById = createdById,
        //        UpdatedById = createdById
        //    };

        //    // Add transactions to the context using repository methods
        //    var debitResult = await AddTransactionAsync(debitTransaction);
        //    if (!debitResult.IsSuccess)
        //    {
        //        return Result<bool>.Failure("debitTransaction failed");
        //    }

        //    var creditResult = await AddTransactionAsync(creditTransaction);
        //    if (!creditResult.IsSuccess)
        //    {
        //        return Result<bool>.Failure("creditTransaction failed");
        //    }

        //    // Update the accounts' balances
        //    sourceAccount.Amount -= amount;
        //    destinationAccount.Amount += amount;

        //    // Save changes
        //    await _context.SaveChangesAsync();

        //    return Result<bool>.Success(true,"Transfer hasbeen successfully done");
        //}

        public async Task<Result<bool>> TransferFundsAsync(long sourceAccountId, long destinationAccountId, decimal amount, string createdById)
        {
            using var dbTransaction = await _context.Database.BeginTransactionAsync();

            try
            {
                var sourceAccount = await _context.Accounts.FirstOrDefaultAsync(a => a.AccountId == sourceAccountId && a.IsActive);
                var destinationAccount = await _context.Accounts.FirstOrDefaultAsync(a => a.AccountId == destinationAccountId && a.IsActive);

                if (sourceAccount == null || destinationAccount == null)
                    return Result<bool>.Failure("Source or destination account not found.");

                if (sourceAccount.Amount < amount)
                    return Result<bool>.Failure("Insufficient balance in source account.");

                var transferCategoryId = await _context.IncomeExpenseCategories
                    .Where(x => x.IncomeExpenseCategoryType.TypeId == 6 && !x.IsDeletable)
                    .Select(x => x.CategoryId)
                    .FirstOrDefaultAsync();

                if (transferCategoryId == 0)
                    return Result<bool>.Failure("Transfer category not configured.");

                // Debit source
                var debitTransaction = new Transaction
                {
                    TransactionDate = DateTime.UtcNow,
                    Amount = -amount,
                    AccountId = sourceAccountId,
                    CategoryId = transferCategoryId,
                    Note = $"Transfer to {destinationAccount.AccountName}",
                    IsActive = true,
                    CreatedById = createdById,
                    UpdatedById = createdById
                };

                // Credit destination
                var creditTransaction = new Transaction
                {
                    TransactionDate = DateTime.UtcNow,
                    Amount = amount,
                    AccountId = destinationAccountId,
                    CategoryId = transferCategoryId,
                    Note = $"Transfer from {sourceAccount.AccountName}",
                    IsActive = true,
                    CreatedById = createdById,
                    UpdatedById = createdById
                };

                // Optionally use transaction repository
                var debitResult = await AddTransactionAsync(debitTransaction);
                if (!debitResult.IsSuccess)
                    return Result<bool>.Failure("Debit transaction failed.");

                var creditResult = await AddTransactionAsync(creditTransaction);
                if (!creditResult.IsSuccess)
                    return Result<bool>.Failure("Credit transaction failed.");

                // Only update balances if you're not doing it inside AddTransactionAsync
                sourceAccount.Amount -= amount;
                destinationAccount.Amount += amount;

                _context.Accounts.Update(sourceAccount);
                _context.Accounts.Update(destinationAccount);

                await _context.SaveChangesAsync();
                await dbTransaction.CommitAsync();

                return Result<bool>.Success(true, "Transfer completed successfully.");
            }
            catch (Exception ex)
            {
                await dbTransaction.RollbackAsync();
                return Result<bool>.Failure("Transfer failed: " + ex.Message);
            }
        }


        #endregion


        #region get all transaction based on account id
        //public async Task<bool> GetAllTransactionbyAccountIdAsync(long accountId)
        //{
        //    var transactionList = await _context.Transactions.Where(x => x.AccountId == accountId && x.IsActive).AnyAsync();
        //    //.Include(t => t.Account)
        //    //       .ThenInclude(ag => ag.AccountGroup)
        //    //.Include(t => t.IncomeExpenseCategory)
        //    //       .ThenInclude(ic => ic.IncomeExpenseCategoryType)
        //    //.Include(t => t.CreatedByUser)
        //    //.Include(t => t.UpdatedByUser)
        //    //.ToListAsync();
        //    return transactionList;
        //}
        #endregion


        public async Task<Result<bool>> DeleteTransactionsByAccountIdAsync(long accountId, string updatedById)
        {
            try
            {
                // Find all active transactions with the given AccountId
                var transactionsToUpdate = await _context.Transactions
                    .Where(t => t.AccountId == accountId && t.IsActive)
                    .ToListAsync();

                if (transactionsToUpdate.Count == 0)
                {
                    return Result<bool>.Failure("No active transactions found for the given account ID.");
                }

                // Set the IsActive property to false (0) for each transaction
                foreach (var transaction in transactionsToUpdate)
                {
                    transaction.IsActive = false;
                    transaction.UpdatedById = updatedById;
                    transaction.UpdatedDate = DateTime.UtcNow;  // Update the UpdatedDate
                }

                // Save changes to the database
                await _context.SaveChangesAsync();

                return Result<bool>.Success(true, "Transaction hasbeen deleted successfully");
            }
            catch (Exception ex)
            {
                return Result<bool>.Failure(ex.Message);
            }
        }


        //public async Task<ResponseResult> DeleteTransactionsAsync(long transactionId, string updatedById)
        //{
        //    try
        //    {
        //        // Find all active transactions with the given AccountId
        //        var transactionsToUpdate = await _context.Transactions
        //            .Where(t => t.TransactionId == transactionId && t.IsActive).FirstOrDefaultAsync();

        //        if (transactionsToUpdate == null)
        //        {
        //            return new ResponseResult { Succeeded = false, Errors = new[] { "No active transactions found." } };
        //        }

        //        var account = await _context.Accounts.FindAsync(transactionsToUpdate.AccountId);
        //        if (account == null)
        //        {
        //            return new ResponseResult { Succeeded = false, Errors = new[] { "Account not found" } };
        //        }

        //        if (transactionsToUpdate.IncomeExpenseCategory.TypeId == 3)
        //        {
        //            account.Amount += -transactionsToUpdate.Amount;
        //            // Step 4: Save the updated account balance
        //            _context.Accounts.Update(account);  // Mark the account for update
        //            await _context.SaveChangesAsync();
        //        }
        //        else if (transactionsToUpdate.IncomeExpenseCategory.TypeId == 5)
        //        {
        //            account.Amount += transactionsToUpdate.Amount;
        //            // Step 4: Save the updated account balance
        //            _context.Accounts.Update(account);  // Mark the account for update
        //            await _context.SaveChangesAsync();
        //        }


        //        // Set the IsActive property to false (0) for each transaction


        //        transactionsToUpdate.IsActive = false;
        //        transactionsToUpdate.UpdatedById = updatedById;
        //        transactionsToUpdate.UpdatedDate = DateTime.UtcNow;  // Update the UpdatedDate


        //        // Save changes to the database
        //        await _context.SaveChangesAsync();

        //        return new ResponseResult { Succeeded = true };
        //    }
        //    catch (Exception ex)
        //    {
        //        return new ResponseResult { Succeeded = false, Errors = new[] { ex.Message } };
        //    }
        //}

    }


}
