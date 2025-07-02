namespace FinTrackHub.Models.DTOs
{
    public class TransactionDto
    {
        public string? EncodedID { get; set; }
        public long? TransactionId { get; set; }
        public long AccountId { get; set; }  // ✅ Needed
        public long CategoryId { get; set; } // ✅ Needed
        public string? CategoryName { get; set; }
        public string? AccountName { get; set; }

        public decimal Amount { get; set; }
        public DateTime? TransactionDate { get; set; }

        public string? Note { get; set; }
        public string? Description { get; set; }
        public string? Attachement { get; set; }

        public string? CreatedBy { get; set; }
        public string? UpdatedBy { get; set; }

        public DateTime? CreatedDate { get; set; }
        public DateTime? UpdatedDate { get; set; }

    }


}
