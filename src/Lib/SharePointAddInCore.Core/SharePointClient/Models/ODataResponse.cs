using Newtonsoft.Json;

namespace SharePointAddInCore.Core.SharePointClient.Models
{
    public class ODataResponse<T>
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
