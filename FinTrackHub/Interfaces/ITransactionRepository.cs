using FinTrackHub.Common;
using FinTrackHub.Entities;
using FinTrackHub.Models.DTOs;


namespace FinTrackHub.Interfaces
{
    public interface ITransactionRepository
    {
         Task<Result<Transaction>> GetTransactionByIdAsync(long TransactionId);    // Read - Select by ID
         Task<Result<Transaction>> AddTransactionAsync(Transaction transaction);   // Create - Insert
         Task<Result<IEnumerable<Transaction>>> GetAllTransactionAsync(string? UserId);        // Read - Select All
         Task<Result<Transaction>> UpdateTransactionAsync(Transaction transaction); // Update
         Task<Result<bool>> DeleteTransactionAsync(long TransactionId, string updatedById);  // Delete
         Task<Result<bool>> TransferFundsAsync(long sourceAccountId, long destinationAccountId, decimal amount, string createdById);
     //    Task<Result<IEnumerable<Transaction>>> GetAllTransactionbyAccountIdAsync(long accountId);        // Read - Select All by accounId
         Task<Result<bool>> DeleteTransactionsByAccountIdAsync(long accountId, string updatedById);
    }

}
