using System;

namespace Bibliotek.LoanAPI.Models
{
    public class LoanEvent
    {
        public int Id { get; set; }
        public int LoanId { get; set; } // Koppling till huvudlånet
        public string Description { get; set; } = string.Empty;
        public DateTime EventDate { get; set; } = DateTime.Now;

        // Navigation property för Entity Framework
        public Loan? Loan { get; set; }
    }
}