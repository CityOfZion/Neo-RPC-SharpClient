using System.Collections.Generic;
using Newtonsoft.Json;

namespace NeoModules.RPC.DTOs
{
    public class ApplicationLog
    {
        [JsonProperty("txid")]
        public string Txid { get; set; }

        [JsonProperty("executions")]
        public List<Execution> Executions { get; set; }
    }

    public class Execution
    {
        [JsonProperty("trigger")]
        public string Trigger { get; set; }

        [JsonProperty("contract")]
        public string Contract { get; set; }

        [JsonProperty("vmstate")]
        public string Vmstate { get; set; }

        [JsonProperty("gas_consumed")]
        public string GasConsumed { get; set; }

        [JsonProperty("stack")]
        public List<Stack> Stack { get; set; }

        [JsonProperty("notifications")]
        public List<Notification> Notifications { get; set; }
    }

    public class Notification
    {
        [JsonProperty("contract")]
        public string Contract { get; set; }

        [JsonProperty("state")]
        public State State { get; set; }
    }

    public class State
    {
        [JsonProperty("type")]
        public string Type { get; set; }

        [JsonProperty("value")]
        public List<Stack> Value { get; set; }
    }
}
