using Daibitx.Security.Hmac.Abstractions;
using Daibitx.Security.Hmac.Models;
using System.Security.Cryptography;
using System.Text;

namespace Daibitx.Security.Hmac.Implementations
{
    public class HmacGenerator : IHmacGenerator
    {
        private readonly IHmacValidator _hmacValidator;

        public HmacGenerator(IHmacValidator hmacValidator)
        {
            _hmacValidator = hmacValidator;
        }

        public (string Timestamp, string BodyHash, string Signature) Generate(
            string method,
            string path,
            string body)
        {

            string timestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString();

            string bodyHash = string.IsNullOrEmpty(body)
                ? ""
                : Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(body))).ToLower();

            string msg = $"{method}:{path}:{timestamp}:{bodyHash}";
            string signature = _hmacValidator.ComputeHmac(msg);

            return (timestamp, bodyHash, signature);
        }

        public HmacHeaderSet GenerateHeaderSet(string method, string path, string body)
        {
            string timestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString();

            string bodyHash = string.IsNullOrEmpty(body)
                ? ""
                : Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(body))).ToLower();

            string msg = $"{method}:{path}:{timestamp}:{bodyHash}";
            string signature = _hmacValidator.ComputeHmac(msg);

            return new HmacHeaderSet
            {
                Timestamp = timestamp,
                BodyHash = bodyHash,
                Signature = signature
            };
        }

    }
}
