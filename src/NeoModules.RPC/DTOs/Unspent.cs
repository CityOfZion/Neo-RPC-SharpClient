using Newtonsoft.Json;

namespace NeoModules.RPC.DTOs
{
    public class Unspent
    {
        [JsonProperty("index")]
        public int Index { get; set; }

        [JsonProperty("txid")]
        public string TransactionId { get; set; }

        [JsonProperty("value")]
        public double Value { get; set; }
    }
}
