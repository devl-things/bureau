using Bureau.Core;
using Microsoft.Extensions.Options;
using Sven.Configurations;
using System.Security.Cryptography;
using System.Text;

namespace Sven.Services
{
    public class AesEncryptor : ISymEncryptor
    {
        private readonly byte[] _key;

        public AesEncryptor(IOptions<EncryptionKeysOptions> options)
        {
            _key = Convert.FromBase64String(options.Value.SymKey);
        }
        public Result<string> Encrypt(int[] ids)
        {
            byte[] input = new byte[ids.Length * sizeof(int)];
            Buffer.BlockCopy(ids, 0, input, 0, input.Length);

            return EncryptWithCombinedIV(input);
        }

        public Result<int[]> DecryptIntArray(string encrypted)
        {
            try
            {
                Result<byte[]> decryptedBytesResult = DecryptWithCombinedIV(encrypted);
                if (decryptedBytesResult.IsError) return decryptedBytesResult.Error;

                byte[] decryptedBytes = decryptedBytesResult.Value;
                int[] result = new int[decryptedBytes.Length / sizeof(int)];
                Buffer.BlockCopy(decryptedBytes, 0, result, 0, decryptedBytes.Length);

                return result;
            }
            catch (Exception ex)
            {
                return new ResultError(ex);
            }
        }


        public Result<string> Encrypt(string[] values)
        {
            string joined = string.Join('\0', values); // null char separator
            byte[] input = Encoding.UTF8.GetBytes(joined);

            return EncryptWithCombinedIV(input);
        }
        public Result<string> Encrypt(string value)
        {
            byte[] input = Encoding.UTF8.GetBytes(value);

            return EncryptWithCombinedIV(input);
        }

        public Result<string> DecryptString(string encrypted)
        {
            Result<string[]> r = DecryptStringArray(encrypted);
            if (r.IsError || r.Value.Length < 1) return r.Error;
            return new Result<string>(r.Value[0]);
        }

        public Result<string[]> DecryptStringArray(string encrypted)
        {
            try
            {
                Result<byte[]> decryptedBytesResult = DecryptWithCombinedIV(encrypted);
                if (decryptedBytesResult.IsError) return decryptedBytesResult.Error;

                byte[] decryptedBytes = decryptedBytesResult.Value;

                string decryptedString = Encoding.UTF8.GetString(decryptedBytes);
                return decryptedString.Split('\0');
            }
            catch (Exception ex)
            {
                return new ResultError(ex);
            }
        }

        private Result<string> EncryptWithCombinedIV(byte[] input)
        {
            try
            {
                using (Aes aes = Aes.Create())
                {
                    aes.Key = _key;
                    aes.GenerateIV();
                    using (ICryptoTransform encryptor = aes.CreateEncryptor())
                    {
                        byte[] encryptedBytes = encryptor.TransformFinalBlock(input, 0, input.Length);

                        byte[] combined = new byte[aes.IV.Length + encryptedBytes.Length];
                        Buffer.BlockCopy(aes.IV, 0, combined, 0, aes.IV.Length);
                        Buffer.BlockCopy(encryptedBytes, 0, combined, aes.IV.Length, encryptedBytes.Length);

                        return new Result<string>(ToUrlSafeBase64(combined));
                    }
                }
            }
            catch (Exception ex)
            {
                return new ResultError(ex);
            }
        }
        private Result<byte[]> DecryptWithCombinedIV(string encrypted)
        {
            try
            {
                byte[] combined = FromUrlSafeBase64(encrypted);
                byte[] iv = new byte[16];
                byte[] ciphertext = new byte[combined.Length - 16];
                Buffer.BlockCopy(combined, 0, iv, 0, iv.Length);
                Buffer.BlockCopy(combined, iv.Length, ciphertext, 0, ciphertext.Length);

                using (Aes aes = Aes.Create())
                {
                    aes.Key = _key;
                    aes.IV = iv;

                    using (ICryptoTransform decryptor = aes.CreateDecryptor())
                    {
                        return decryptor.TransformFinalBlock(ciphertext, 0, ciphertext.Length);
                    }
                }
            }
            catch (Exception ex)
            {
                return new ResultError(ex);
            }
        }

        private static string ToUrlSafeBase64(byte[] bytes)
        {
            return Convert.ToBase64String(bytes)
                          .TrimEnd('=')              // optional padding removal
                          .Replace('+', '-')
                          .Replace('/', '_');
        }

        private static byte[] FromUrlSafeBase64(string base64Url)
        {
            string padded = base64Url.Replace('-', '+')
                                     .Replace('_', '/');

            // Add back missing padding if needed
            switch (padded.Length % 4)
            {
                case 2: padded += "=="; break;
                case 3: padded += "="; break;
            }

            return Convert.FromBase64String(padded);
        }
    }
}
