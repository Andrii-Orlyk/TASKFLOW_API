using Microsoft.AspNetCore.Identity;
using TaskFlow.Application.Interfaces;
using TaskFlow.Domain.Entities;

namespace TaskFlow.Infrastructure.Auth;

public sealed class PasswordHasher : IPasswordHasher
{
    private readonly Microsoft.AspNetCore.Identity.PasswordHasher<User> _passwordHasher = new();

    public string Hash(string password) => _passwordHasher.HashPassword(user: null!, password);

    public bool Verify(string password, string passwordHash) =>
        _passwordHasher.VerifyHashedPassword(user: null!, passwordHash, password)
        != PasswordVerificationResult.Failed;
}
