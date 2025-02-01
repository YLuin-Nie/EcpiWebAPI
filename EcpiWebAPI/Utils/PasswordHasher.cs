using BCrypt.Net;

public static class PasswordHasher
{
    // 🔹 Hash a password with a salt
    public static string HashPassword(string password)
    {
        return BCrypt.Net.BCrypt.HashPassword(password);
    }

    // 🔹 Verify if a password matches the hash
    public static bool VerifyPassword(string password, string hashedPassword)
    {
        return BCrypt.Net.BCrypt.Verify(password, hashedPassword);
    }
}
