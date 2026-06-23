using System.Security.Cryptography;
using HospitalApp.Core.Application.Common;
using Microsoft.Extensions.Configuration;

namespace HospitalApp.Infrastructure.Shared.Services;

public class AesGcmPiiProtector(IConfiguration configuration) : IPiiProtector
{
    private const int NonceSize = 12;
    private const int TagSize = 16;
    private readonly byte[] _key = LoadKey(configuration);

    public string Protect(string plaintext)
    {
        var nonce = RandomNumberGenerator.GetBytes(NonceSize);
        var plaintextBytes = System.Text.Encoding.UTF8.GetBytes(plaintext);
        var ciphertext = new byte[plaintextBytes.Length];
        var tag = new byte[TagSize];

        using var aes = new AesGcm(_key, TagSize);
        aes.Encrypt(nonce, plaintextBytes, ciphertext, tag);

        var payload = new byte[1 + nonce.Length + tag.Length + ciphertext.Length];
        payload[0] = 1;
        nonce.CopyTo(payload, 1);
        tag.CopyTo(payload, 1 + nonce.Length);
        ciphertext.CopyTo(payload, 1 + nonce.Length + tag.Length);

        return Convert.ToBase64String(payload);
    }

    public string? ProtectNullable(string? plaintext) =>
        string.IsNullOrEmpty(plaintext) ? plaintext : Protect(plaintext);

    public string Unprotect(string protectedValue)
    {
        var payload = Convert.FromBase64String(protectedValue);
        if (payload.Length < 1 + NonceSize + TagSize || payload[0] != 1)
            throw new CryptographicException("Unsupported PII payload format.");

        var nonce = payload.AsSpan(1, NonceSize);
        var tag = payload.AsSpan(1 + NonceSize, TagSize);
        var ciphertext = payload.AsSpan(1 + NonceSize + TagSize);
        var plaintext = new byte[ciphertext.Length];

        using var aes = new AesGcm(_key, TagSize);
        aes.Decrypt(nonce, ciphertext, tag, plaintext);

        return System.Text.Encoding.UTF8.GetString(plaintext);
    }

    public string? UnprotectNullable(string? protectedValue) =>
        string.IsNullOrEmpty(protectedValue) ? protectedValue : Unprotect(protectedValue);

    private static byte[] LoadKey(IConfiguration configuration)
    {
        var raw = configuration["PiiProtection:Key"];
        if (string.IsNullOrWhiteSpace(raw))
            throw new InvalidOperationException("PiiProtection:Key must be configured.");

        var key = Convert.FromBase64String(raw);
        if (key.Length is not (16 or 24 or 32))
            throw new InvalidOperationException("PiiProtection:Key must be a base64 AES key of 128, 192, or 256 bits.");

        return key;
    }
}
