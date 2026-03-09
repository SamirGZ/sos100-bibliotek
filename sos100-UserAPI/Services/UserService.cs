using Microsoft.EntityFrameworkCore;

public class UserService : IUserService
{
    private readonly AppDbContext _context;

    public UserService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<User?> AuthenticateAsync(string username, string password)
    {
        var user = await _context.Users
            .FirstOrDefaultAsync(u => u.Username == username);

        if (user == null) return null;

        // Verify hashed password
        bool isValid = BCrypt.Net.BCrypt.Verify(password, user.PasswordHash);
        return isValid ? user : null;
    }

    public async Task<bool> RegisterAsync(string username, string password, string email)
    {
        Console.WriteLine($"Attempting to register user: '{username}'");
    
        var exists = await _context.Users.AnyAsync(u => u.Username == username);
        Console.WriteLine($"User exists check: {exists}");
    
        if (exists) return false;

        var user = new User
        {
            Username = username,
            Email = email,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(password)
        };

        _context.Users.Add(user);
        await _context.SaveChangesAsync();
    
        Console.WriteLine($"User '{username}' registered successfully!");
        return true;
    }
}