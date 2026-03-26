namespace sos100_bibliotek.Models
{
    public class LoanViewModel
    {
        public int Id { get; set; }
        public int UserId { get; set; } 
        public int BookId { get; set; }
        public string BookTitle { get; set; } = string.Empty;
        public string Username { get; set; } = string.Empty; 
        public DateTime LoanDate { get; set; }
        public DateTime ReturnDate { get; set; }
        public bool IsReturned { get; set; }
    }
}