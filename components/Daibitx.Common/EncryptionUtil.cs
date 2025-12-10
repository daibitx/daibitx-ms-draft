using System;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;

namespace Daibitx.Common
{
    /// <summary>
    /// Encryption utility class
    /// Provides various encryption, encoding and decoding methods
    /// </summary>
    public static class EncryptionUtil
    {
        private const int SaltSize = 16;
        private const int HashSize = 20;
        private const int Iterations = 10000;

        /// <summary>
        /// MD5 encryption
        /// </summary>
        /// <param name="input">String to encrypt</param>
        /// <returns>MD5 hash hexadecimal string</returns>
        public static string Md5(string input)
        {
            if (StringUtil.IsNullOrWhiteSpace(input))
            {
                return string.Empty;
            }

            using (var md5 = MD5.Create())
            {
                var inputBytes = Encoding.UTF8.GetBytes(input);
                var hashBytes = md5.ComputeHash(inputBytes);
                
                var sb = new StringBuilder();
                foreach (var b in hashBytes)
                {
                    sb.Append(b.ToString("x2"));
                }
                
                return sb.ToString();
            }
        }

        /// <summary>
        /// SHA1 encryption
        /// </summary>
        /// <param name="input">String to encrypt</param>
        /// <returns>SHA1 hash hexadecimal string</returns>
        public static string Sha1(string input)
        {
            if (StringUtil.IsNullOrWhiteSpace(input))
            {
                return string.Empty;
            }

            using (var sha1 = SHA1.Create())
            {
                var inputBytes = Encoding.UTF8.GetBytes(input);
                var hashBytes = sha1.ComputeHash(inputBytes);
                
                var sb = new StringBuilder();
                foreach (var b in hashBytes)
                {
                    sb.Append(b.ToString("x2"));
                }
                
                return sb.ToString();
            }
        }

        /// <summary>
        /// SHA256 encryption
        /// </summary>
        /// <param name="input">String to encrypt</param>
        /// <returns>SHA256 hash hexadecimal string</returns>
        public static string Sha256(string input)
        {
            if (StringUtil.IsNullOrWhiteSpace(input))
            {
                return string.Empty;
            }

            using (var sha256 = SHA256.Create())
            {
                var inputBytes = Encoding.UTF8.GetBytes(input);
                var hashBytes = sha256.ComputeHash(inputBytes);
                
                var sb = new StringBuilder();
                foreach (var b in hashBytes)
                {
                    sb.Append(b.ToString("x2"));
                }
                
                return sb.ToString();
            }
        }

        /// <summary>
        /// SHA512 encryption
        /// </summary>
        /// <param name="input">String to encrypt</param>
        /// <returns>SHA512 hash hexadecimal string</returns>
        public static string Sha512(string input)
        {
            if (StringUtil.IsNullOrWhiteSpace(input))
            {
                return string.Empty;
            }

            using (var sha512 = SHA512.Create())
            {
                var inputBytes = Encoding.UTF8.GetBytes(input);
                var hashBytes = sha512.ComputeHash(inputBytes);
                
                var sb = new StringBuilder();
                foreach (var b in hashBytes)
                {
                    sb.Append(b.ToString("x2"));
                }
                
                return sb.ToString();
            }
        }

        /// <summary>
        /// Base64 encoding
        /// </summary>
        /// <param name="input">String to encode</param>
        /// <returns>Base64 encoded string</returns>
        public static string Base64Encode(string input)
        {
            if (StringUtil.IsNullOrWhiteSpace(input))
            {
                return string.Empty;
            }

            try
            {
                var inputBytes = Encoding.UTF8.GetBytes(input);
                return Convert.ToBase64String(inputBytes);
            }
            catch
            {
                return string.Empty;
            }
        }

        /// <summary>
        /// Base64 decoding
        /// </summary>
        /// <param name="input">Base64 string to decode</param>
        /// <returns>Decoded string</returns>
        public static string Base64Decode(string input)
        {
            if (StringUtil.IsNullOrWhiteSpace(input))
            {
                return string.Empty;
            }

            try
            {
                var decodedBytes = Convert.FromBase64String(input);
                return Encoding.UTF8.GetString(decodedBytes);
            }
            catch
            {
                return string.Empty;
            }
        }

        /// <summary>
        /// URL encoding
        /// </summary>
        /// <param name="input">String to encode</param>
        /// <returns>URL encoded string</returns>
        public static string UrlEncode(string input)
        {
            if (StringUtil.IsNullOrWhiteSpace(input))
            {
                return string.Empty;
            }

            try
            {
                return Uri.EscapeDataString(input);
            }
            catch
            {
                return string.Empty;
            }
        }

        /// <summary>
        /// URL decoding
        /// </summary>
        /// <param name="input">URL string to decode</param>
        /// <returns>Decoded string</returns>
        public static string UrlDecode(string input)
        {
            if (StringUtil.IsNullOrWhiteSpace(input))
            {
                return string.Empty;
            }

            try
            {
                return Uri.UnescapeDataString(input);
            }
            catch
            {
                return string.Empty;
            }
        }

        /// <summary>
        /// Generate salt of specified length
        /// </summary>
        /// <param name="size">Salt length (bytes)</param>
        /// <returns>Base64 encoded salt</returns>
        public static string GenerateSalt(int size = SaltSize)
        {
            if (size <= 0)
            {
                size = SaltSize;
            }

            var saltBytes = new byte[size];
            using (var rng = new RNGCryptoServiceProvider())
            {
                rng.GetBytes(saltBytes);
            }

            return Convert.ToBase64String(saltBytes);
        }

        /// <summary>
        /// Hash password (using PBKDF2)
        /// </summary>
        /// <param name="password">Password</param>
        /// <param name="salt">Salt</param>
        /// <returns>Hashed password</returns>
        public static string HashPassword(string password, string salt)
        {
            if (StringUtil.IsNullOrWhiteSpace(password))
            {
                return string.Empty;
            }

            if (StringUtil.IsNullOrWhiteSpace(salt))
            {
                salt = GenerateSalt();
            }

            try
            {
                var saltBytes = Convert.FromBase64String(salt);
                using (var pbkdf2 = new Rfc2898DeriveBytes(password, saltBytes, Iterations))
                {
                    var hashBytes = pbkdf2.GetBytes(HashSize);
                    return Convert.ToBase64String(hashBytes);
                }
            }
            catch
            {
                return string.Empty;
            }
        }

        /// <summary>
        /// Verify password
        /// </summary>
        /// <param name="password">Password to verify</param>
        /// <param name="hashedPassword">Hashed password</param>
        /// <returns>Whether password matches</returns>
        public static bool VerifyPassword(string password, string hashedPassword)
        {
            if (StringUtil.IsNullOrWhiteSpace(password) || StringUtil.IsNullOrWhiteSpace(hashedPassword))
            {
                return false;
            }

            // Simple password verification logic, should use more secure password storage in real applications
            var inputHash = Sha256(password);
            return string.Equals(inputHash, hashedPassword, StringComparison.OrdinalIgnoreCase);
        }
    }
}