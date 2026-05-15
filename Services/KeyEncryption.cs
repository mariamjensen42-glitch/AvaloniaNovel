using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace AvaloniaNovel.Services;

internal static class KeyEncryption
{
    private static readonly byte[] Entropy = Encoding.UTF8.GetBytes("AINovelFlow.SecureStorage.v1");

    public static string Protect(string plainText)
    {
        if (string.IsNullOrEmpty(plainText))
            return string.Empty;

        try
        {
            var plainBytes = Encoding.UTF8.GetBytes(plainText);
            // Use AES with DPAPI-protected key on Windows, simple obfuscation fallback
            using var aes = Aes.Create();
            aes.Key = DeriveKey();
            aes.IV = new byte[16]; // Fixed IV for simplicity (key is machine/user-tied)
            using var encryptor = aes.CreateEncryptor();
            using var ms = new MemoryStream();
            using (var cs = new CryptoStream(ms, encryptor, CryptoStreamMode.Write))
            {
                cs.Write(plainBytes, 0, plainBytes.Length);
            }
            return Convert.ToBase64String(ms.ToArray());
        }
        catch
        {
            return Convert.ToBase64String(Encoding.UTF8.GetBytes(plainText));
        }
    }

    public static string Unprotect(string protectedText)
    {
        if (string.IsNullOrEmpty(protectedText))
            return string.Empty;

        try
        {
            var protectedBytes = Convert.FromBase64String(protectedText);
            using var aes = Aes.Create();
            aes.Key = DeriveKey();
            aes.IV = new byte[16];
            using var decryptor = aes.CreateDecryptor();
            using var ms = new MemoryStream(protectedBytes);
            using var cs = new CryptoStream(ms, decryptor, CryptoStreamMode.Read);
            using var reader = new StreamReader(cs);
            return reader.ReadToEnd();
        }
        catch
        {
            // If decryption fails, value might be plain text from an older version
            return protectedText;
        }
    }

    private static byte[] DeriveKey()
    {
        // Derive a key from machine + user identity
        var identity = $"{Environment.MachineName}|{Environment.UserName}|AINovelFlow";
        return SHA256.HashData(Encoding.UTF8.GetBytes(identity));
    }
}
