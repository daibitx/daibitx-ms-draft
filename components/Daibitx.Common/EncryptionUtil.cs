using System;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;

namespace Daibitx.Common
{
    /// <summary>
    /// 加密工具类
    /// 提供各种加密、编码和解码方法
    /// </summary>
    public static class EncryptionUtil
    {
        private const int SaltSize = 16;
        private const int HashSize = 20;
        private const int Iterations = 10000;

        /// <summary>
        /// MD5加密
        /// </summary>
        /// <param name="input">要加密的字符串</param>
        /// <returns>MD5哈希值的十六进制字符串</returns>
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
        /// SHA1加密
        /// </summary>
        /// <param name="input">要加密的字符串</param>
        /// <returns>SHA1哈希值的十六进制字符串</returns>
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
        /// SHA256加密
        /// </summary>
        /// <param name="input">要加密的字符串</param>
        /// <returns>SHA256哈希值的十六进制字符串</returns>
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
        /// SHA512加密
        /// </summary>
        /// <param name="input">要加密的字符串</param>
        /// <returns>SHA512哈希值的十六进制字符串</returns>
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
        /// Base64编码
        /// </summary>
        /// <param name="input">要编码的字符串</param>
        /// <returns>Base64编码后的字符串</returns>
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
        /// Base64解码
        /// </summary>
        /// <param name="input">要解码的Base64字符串</param>
        /// <returns>解码后的字符串</returns>
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
        /// URL编码
        /// </summary>
        /// <param name="input">要编码的字符串</param>
        /// <returns>URL编码后的字符串</returns>
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
        /// URL解码
        /// </summary>
        /// <param name="input">要解码的URL字符串</param>
        /// <returns>解码后的字符串</returns>
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
        /// 生成指定长度的盐值
        /// </summary>
        /// <param name="size">盐值长度（字节）</param>
        /// <returns>Base64编码的盐值</returns>
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
        /// 哈希密码（使用PBKDF2）
        /// </summary>
        /// <param name="password">密码</param>
        /// <param name="salt">盐值</param>
        /// <returns>哈希后的密码</returns>
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
        /// 验证密码
        /// </summary>
        /// <param name="password">要验证的密码</param>
        /// <param name="hashedPassword">已哈希的密码</param>
        /// <returns>密码是否匹配</returns>
        public static bool VerifyPassword(string password, string hashedPassword)
        {
            if (StringUtil.IsNullOrWhiteSpace(password) || StringUtil.IsNullOrWhiteSpace(hashedPassword))
            {
                return false;
            }

            // 简单的密码验证逻辑，实际应用中应该使用更安全的密码存储方式
            var inputHash = Sha256(password);
            return string.Equals(inputHash, hashedPassword, StringComparison.OrdinalIgnoreCase);
        }
    }
}