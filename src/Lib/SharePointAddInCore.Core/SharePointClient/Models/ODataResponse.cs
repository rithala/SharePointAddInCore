using Newtonsoft.Json;

namespace SharePointAddInCore.Core.SharePointClient.Models
{
    internal class ODataResponse<T>
    {
        [JsonProperty("d")]
        public T Data { get; }

        [JsonConstructor]
        public ODataResponse(T data)
        {
            Data = data;
        }
    }
}
