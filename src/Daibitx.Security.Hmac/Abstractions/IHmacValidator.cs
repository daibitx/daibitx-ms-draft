using Daibitx.Security.Hmac.Models;

namespace Daibitx.Security.Hmac.Abstractions
{
    public interface IHmacValidator
    {
        string ComputeHmac(string message);
        bool Validate(string method, string path, string timestamp, string bodyHash, string signature);
        HmacHeaderSet ComputeHmac(string method, string path, string timestamp, string bodyHash, string signture);
    }
}