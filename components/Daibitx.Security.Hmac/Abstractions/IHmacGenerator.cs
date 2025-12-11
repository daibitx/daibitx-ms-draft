using Daibitx.Security.Hmac.Models;

namespace Daibitx.Security.Hmac.Abstractions
{
    public interface IHmacGenerator
    {
        (string Timestamp, string BodyHash, string Signature) Generate(string method, string path, string body);
        HmacHeaderSet GenerateHeaderSet(string method, string path, string body);
    }
}