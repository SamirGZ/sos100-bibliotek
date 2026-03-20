using System;
using System.Text.Json.Serialization; // <--- Lägg till denna using!

namespace Bibliotek.LoanAPI.Models
{
    public class LoanEvent
    {
        public int Id { get; set; }
        public int LoanId { get; set; }
        public string Description { get; set; } = string.Empty;
        public DateTime EventDate { get; set; } = DateTime.Now;

        // Navigation property för Entity Framework
        [JsonIgnore] // <--- DENNA RAD STOPPAR KRASCHEN!
        public Loan? Loan { get; set; }
    }
}