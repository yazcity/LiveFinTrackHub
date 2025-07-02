using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;

namespace FinTrackHub.Models.DTOs
{
    public class TransferDto
    {
        [Required(ErrorMessage = "Transaction Date is required.")]
        [DataType(DataType.Date)]
        public DateTime TransactionDate { get; set; } = DateTime.Now;
        [Required(ErrorMessage = "Amount is required.")]
        public decimal Amount { get; set; }
        [Required(ErrorMessage = "From Account is required.")]
        public long sourceAccountId { get; set; }
        [Required(ErrorMessage = "To account is required.")]
        public long destinationAccountId { get; set; }
        public string? Description { get; set; }
        public string? Attachement { get; set; }
    }
}
