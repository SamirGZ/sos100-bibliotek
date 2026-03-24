using System.Text.Json.Serialization; // Lägg till denna using!

namespace Bibliotek.LoanAPI.Models
{
    public class LoanEvent
    {
        public int Id { get; set; }
        public string Description { get; set; }
        public DateTime EventDate { get; set; }

        public int LoanId { get; set; }

        [JsonIgnore] // <--- LÄGG TILL DENNA RAD!
        public Loan Loan { get; set; } 
    }
}