using System.Text.Json.Serialization;
using System.Text.Json;

namespace HassMqtt.Support {
    public class JsonPolicy : JsonNamingPolicy {
        public override string ConvertName( string name ) => name.ToLowerInvariant();

        public static readonly JsonSerializerOptions JsonSerializerOptions = new() {
            PropertyNamingPolicy = new JsonPolicy(),
            PropertyNameCaseInsensitive = true,
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
            Converters = { new JsonStringEnumConverter() }
        };
    }
}
