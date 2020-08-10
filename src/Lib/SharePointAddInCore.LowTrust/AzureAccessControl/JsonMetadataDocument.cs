using Newtonsoft.Json;

using System.Collections.Generic;

namespace SharePointAddInCore.LowTrust.AzureAccessControl
{
    internal class JsonMetadataDocument
    {
        [JsonProperty("serviceName")]
        public string ServiceName { get; set; }
        [JsonProperty("endpoints")]
        public List<JsonEndpoint> Endpoints { get; set; }
        [JsonProperty("keys")]
        public List<JsonKey> Keys { get; set; }
    }

    internal class JsonEndpoint
    {
        [JsonProperty("location")]
        public string Location { get; set; }
        [JsonProperty("protocol")]
        public string Protocol { get; set; }
        [JsonProperty("usage")]
        public string Usage { get; set; }
    }

    internal class JsonKeyValue
    {
        [JsonProperty("type")]
        public string Type { get; set; }
        [JsonProperty("value")]
        public string Value { get; set; }
    }

    internal class JsonKey
    {
        [JsonProperty("usage")]
        public string Usage { get; set; }
        [JsonProperty("keyValue")]
        public JsonKeyValue KeyValue { get; set; }
    }
}
