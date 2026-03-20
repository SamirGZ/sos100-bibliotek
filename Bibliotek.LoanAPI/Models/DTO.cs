namespace Bibliotek.LoanAPI.Models
{
    public class CreateLoanDto
    {
        public int BookId { get; set; }
        public int UserId { get; set; }
    }
}