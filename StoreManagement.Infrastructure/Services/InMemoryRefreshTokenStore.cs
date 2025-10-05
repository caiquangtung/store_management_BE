using System.Collections.Concurrent;
using System.Security.Cryptography;
using System.Text;
using StoreManagement.Application.Common.Interfaces;

namespace StoreManagement.Infrastructure.Services;

public class InMemoryRefreshTokenStore : IRefreshTokenStore
{
    private class Entry
    {
        public int UserId { get; set; }
        public DateTime ExpiresAt { get; set; }
        public bool Revoked { get; set; }
    }

    // key: tokenHash (SHA256 of raw), value: entry
    private readonly ConcurrentDictionary<string, Entry> _tokens = new();

    public Task<string> IssueTokenAsync(int userId, TimeSpan lifetime)
    {
        var raw = GenerateSecureToken(64); // 64 bytes -> 128 hex chars
        var hash = ComputeSha256(raw);
        var entry = new Entry
        {
            UserId = userId,
            ExpiresAt = DateTime.UtcNow.Add(lifetime),
            Revoked = false
        };
        _tokens[hash] = entry;
        return Task.FromResult(raw);
    }

    public Task<int?> ValidateTokenAsync(string refreshToken)
    {
        var hash = ComputeSha256(refreshToken);
        if (_tokens.TryGetValue(hash, out var entry))
        {
            if (!entry.Revoked && entry.ExpiresAt > DateTime.UtcNow)
            {
                return Task.FromResult<int?>(entry.UserId);
            }
        }
        return Task.FromResult<int?>(null);
    }

    public Task<bool> RevokeTokenAsync(string refreshToken)
    {
        var hash = ComputeSha256(refreshToken);
        if (_tokens.TryGetValue(hash, out var entry))
        {
            entry.Revoked = true;
            return Task.FromResult(true);
        }
        return Task.FromResult(false);
    }

    public Task<int> RevokeAllForUserAsync(int userId)
    {
        var count = 0;
        foreach (var kv in _tokens)
        {
            if (kv.Value.UserId == userId && !kv.Value.Revoked)
            {
                kv.Value.Revoked = true;
                count++;
            }
        }
        return Task.FromResult(count);
    }

    public Task<DateTime?> GetExpiryAsync(string refreshToken)
    {
        var hash = ComputeSha256(refreshToken);
        if (_tokens.TryGetValue(hash, out var entry))
        {
            return Task.FromResult<DateTime?>(entry.ExpiresAt);
        }
        return Task.FromResult<DateTime?>(null);
    }

    private static string GenerateSecureToken(int numBytes)
    {
        var bytes = RandomNumberGenerator.GetBytes(numBytes);
        return Convert.ToHexString(bytes);
    }

    private static string ComputeSha256(string input)
    {
        var bytes = Encoding.UTF8.GetBytes(input);
        var hash = SHA256.HashData(bytes);
        return Convert.ToHexString(hash);
    }
}


