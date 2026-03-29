public interface IUserService
{
    Task<User?> AuthenticateAsync(string username, string password);
    Task<bool> RegisterAsync(string username, string password, string email);
    Task<User?> GetUserByIdAsync(int id);
    Task<User?> GetUserAsync(string username);
    Task<bool> DeleteUserAsync(string username);
    Task<bool> UpdatePasswordAsync(string username, string newPassword);
}