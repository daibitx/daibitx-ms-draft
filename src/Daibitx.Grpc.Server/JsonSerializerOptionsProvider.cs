using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Daibitx.Grpc.Server
{
    public class JsonSerializerOptionsProvider
    {
        public JsonSerializerOptions DefaultOptions { get; }

        public JsonSerializerOptionsProvider(Action<JsonSerializerOptions> action = null)
        {

            DefaultOptions = new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
                WriteIndented = false,
                PropertyNameCaseInsensitive = true,
                Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
                NumberHandling = JsonNumberHandling.AllowReadingFromString
            };
            action?.Invoke(DefaultOptions);
        }
    }
}
