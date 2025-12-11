namespace Daibitx.Security.Hmac.Models
{
    public class HmacHeaderSet
    {
        public string Timestamp { get; set; } = "";
        public string BodyHash { get; set; } = "";
        public string Signature { get; set; } = "";

        public IDictionary<string, string> ToDictionary() =>
            new Dictionary<string, string>
            {
                ["X-GW-Timestamp"] = Timestamp,
                ["X-GW-BodyHash"] = BodyHash,
                ["X-GW-Signature"] = Signature
            };
    }

}
