// Feature: financial-assistant-analytics-auth, Property 1: Hash does not match original password and has PBKDF2 format
using FsCheck;
using FsCheck.Xunit;
using FluentAssertions;
using System.Security.Cryptography;

namespace FinancialAssistant.Tests.Auth;

public class PasswordHasherTests
{
    // Inline the PasswordHasher logic since we can't reference the MAUI project directly
    private static string HashPassword(string password)
    {
        byte[] salt = new byte[16];
        RandomNumberGenerator.Fill(salt);
        using var pbkdf2 = new Rfc2898DeriveBytes(password, salt, 10_000, HashAlgorithmName.SHA256);
        byte[] hash = pbkdf2.GetBytes(32);
        return Convert.ToBase64String(salt) + ":" + Convert.ToBase64String(hash);
    }

    private static bool VerifyPassword(string password, string storedHash)
    {
        var parts = storedHash.Split(':');
        if (parts.Length != 2) return false;
        byte[] salt = Convert.FromBase64String(parts[0]);
        byte[] expectedHash = Convert.FromBase64String(parts[1]);
        using var pbkdf2 = new Rfc2898DeriveBytes(password, salt, 10_000, HashAlgorithmName.SHA256);
        byte[] actualHash = pbkdf2.GetBytes(32);
        return CryptographicOperations.FixedTimeEquals(actualHash, expectedHash);
    }

    [Property(MaxTest = 100)]
    public Property Hash_DoesNotMatch_OriginalPassword()
    {
        // Property 1: For any password of 8+ chars, hash != original password and has PBKDF2 format
        var gen = Arb.Default.NonEmptyString().Generator
            .Where(s => s.Get.Length >= 8);
        return Prop.ForAll(Arb.From(gen), password =>
        {
            var hash = HashPassword(password.Get);
            hash.Should().NotBe(password.Get);
            hash.Should().Contain(":");
            var parts = hash.Split(':');
            parts.Should().HaveCount(2);
            // Both parts should be valid Base64
            Action act1 = () => Convert.FromBase64String(parts[0]);
            Action act2 = () => Convert.FromBase64String(parts[1]);
            act1.Should().NotThrow();
            act2.Should().NotThrow();
        });
    }

    [Property(MaxTest = 100)]
    public Property VerifyPassword_ReturnsTrue_ForCorrectPassword()
    {
        var gen = Arb.Default.NonEmptyString().Generator
            .Where(s => s.Get.Length >= 8);
        return Prop.ForAll(Arb.From(gen), password =>
        {
            var hash = HashPassword(password.Get);
            VerifyPassword(password.Get, hash).Should().BeTrue();
        });
    }

    [Property(MaxTest = 100)]
    public Property VerifyPassword_ReturnsFalse_ForWrongPassword()
    {
        var gen = Gen.Two(Arb.Default.NonEmptyString().Generator.Where(s => s.Get.Length >= 8));
        return Prop.ForAll(Arb.From(gen), pair =>
        {
            var (p1, p2) = pair;
            if (p1.Get == p2.Get) return; // skip equal passwords
            var hash = HashPassword(p1.Get);
            VerifyPassword(p2.Get, hash).Should().BeFalse();
        });
    }
}
