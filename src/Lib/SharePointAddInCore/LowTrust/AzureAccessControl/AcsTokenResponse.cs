using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

using System;

namespace SharePointAddInCore.LowTrust.AzureAccessControl
{
    internal class AcsTokenResponse
    {
        [JsonProperty("access_token")]
        public string AccessToken { get; set; }

        [JsonProperty("token_type")]
        public string TokenType { get; set; }

        [JsonProperty("expires_in")]
        [JsonConverter(typeof(LongTimeSpanConverter))]
        public TimeSpan ExpiresIn { get; set; }

        [JsonProperty("expires_on")]
        [JsonConverter(typeof(SecondsEpochConverter))]
        public DateTime ExpiresOn { get; set; }

        [JsonProperty("not_before")]
        [JsonConverter(typeof(SecondsEpochConverter))]
        public DateTime NotBefore { get; set; }

        [JsonProperty("resource")]
        public string Resource { get; set; }
    }

    internal class LongTimeSpanConverter : JsonConverter
    {
        public override bool CanConvert(Type objectType)
        {
            return objectType == typeof(string);
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            if (reader.Value == null) { return null; }
            return TimeSpan.FromSeconds(long.Parse(reader.Value.ToString()));
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            writer.WriteRawValue(((TimeSpan)value).TotalSeconds.ToString());
        }
    }

    internal class SecondsEpochConverter : DateTimeConverterBase
    {
        private static readonly DateTime _epoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            writer.WriteRawValue(((DateTime)value - _epoch).TotalSeconds.ToString());
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            if (reader.Value == null) { return null; }
            return _epoch.AddSeconds(long.Parse(reader.Value.ToString()));
        }
    }
}
