using System.Collections.Generic;
using Newtonsoft.Json;

namespace NeoModules.Rest.DTOs.NeoScan
{
    public class AddressBalance
    {
        [JsonProperty("balance")]
        public IList<Balance> Balance { get; set; }

        [JsonProperty("address")]
        public string Address { get; set; }

        public static AddressBalance FromJson(string json) => JsonConvert.DeserializeObject<AddressBalance>(json,Utils.Settings);
    }

    public class Balance
    {
        [JsonProperty("unspent")]
        public IList<Unspent> Unspent { get; set; }

        [JsonProperty("asset_symbol")]
        public string AssetSymbol { get; set; }

        [JsonProperty("asset_hash")]
        public string AssetHash { get; set; }

        [JsonProperty("asset")]
        public string Asset { get; set; }

        [JsonProperty("amount")]
        public float Amount { get; set; }
    }

    public class Unspent
    {
        [JsonProperty("value")]
        public decimal Value { get; set; }

        [JsonProperty("txid")]
        public string TxId { get; set; }

        [JsonProperty("n")]
        public uint N { get; set; }
    }
}