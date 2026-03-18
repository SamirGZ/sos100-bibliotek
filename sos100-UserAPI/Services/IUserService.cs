public interface IUserService
{
    Task<User?> AuthenticateAsync(string username, string password);
    Task<bool> RegisterAsync(string username, string password, string email);
}