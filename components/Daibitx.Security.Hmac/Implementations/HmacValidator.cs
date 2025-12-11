using Daibitx.Security.Hmac.Abstractions;
using Daibitx.Security.Hmac.Models;
using Microsoft.Extensions.Options;
using System.Security.Cryptography;
using System.Text;

namespace Daibitx.Security.Hmac.Implementations
{
    public class HmacValidator : IHmacValidator
    {
        private readonly IOptionsMonitor<HmacOptions> _options;

        public HmacValidator(IOptionsMonitor<HmacOptions> options)
        {
            _options = options;
        }


        public bool Validate(string method, string path, string timestamp, string bodyHash, string signature)
        {
            var opts = _options.CurrentValue;
            if (string.IsNullOrEmpty(opts.SecretKey))
            {
                return false;
            }
            if (!long.TryParse(timestamp, out long ts))
            {
                return false;
            }

            long now = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
            if (Math.Abs(now - ts) > opts.AllowedTimestampDriftSeconds)
            {
                return false;
            }
            string msg = $"{method}:{path}:{timestamp}:{bodyHash}";
            string expected = ComputeHmac(msg);

            return CryptographicOperations.FixedTimeEquals(
                Encoding.UTF8.GetBytes(expected),
                Encoding.UTF8.GetBytes(signature));

        }

        public string ComputeHmac(string message)
        {
            using var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(_options.CurrentValue.SecretKey));
            var hash = hmac.ComputeHash(Encoding.UTF8.GetBytes(message));
            return Convert.ToHexString(hash).ToLower();
        }
    }
}
