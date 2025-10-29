namespace Barberly.Application.Interfaces;

/// <summary>
/// Provides password hashing and verification helpers for authentication workflows.
/// </summary>
public interface IPasswordHasher
{
    string HashPassword(string password);
    bool VerifyPassword(string password, string hashedPassword);
}

