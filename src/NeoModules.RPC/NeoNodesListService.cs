using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace NeoModules.RPC
{
    // simple service, don't need to separate DTO's and the service does not belong in the same context as RPC
    public class NeoNodesListService
    {
        private const string MonitorCityOfZionTestNet = "http://monitor.cityofzion.io/assets/testnet.json";
        private const string MonitorCityOfZionMainNet = "http://monitor.cityofzion.io/assets/mainnet.json";

        public async Task<string> GetNodesList(MonitorNet net)
        {
            var restClient = new HttpClient();
            HttpResponseMessage resultMessage;
            if (net == MonitorNet.MainNet)
                resultMessage = await restClient.GetAsync(MonitorCityOfZionMainNet);
            else
                resultMessage = await restClient.GetAsync(MonitorCityOfZionTestNet);
            var data = await resultMessage.Content.ReadAsStringAsync();
            return data;
        }
    }

    public enum MonitorNet
    {
        MainNet,
        TestNet
    }

    public enum Protocol
    {
        Http,
        Https
    }

    public enum SiteType
    {
        Rest,
        Rpc,
        WebSockets
    }

    public class NodeList
    {
        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("pollTime")]
        public string PollTime { get; set; }

        [JsonProperty("sites")]
        public List<Site> Sites { get; set; }
    }

    public class Site
    {
        [JsonProperty("service")]
        public string Service { get; set; }

        [JsonProperty("url")]
        public string Url { get; set; }

        [JsonProperty("location")]
        public string Location { get; set; }

        [JsonProperty("address")]
        public string Address { get; set; }

        [JsonProperty("locale")]
        public string Locale { get; set; }

        [JsonProperty("type")]
        public SiteType Type { get; set; }

        [JsonProperty("protocol")]
        public Protocol? Protocol { get; set; }

        [JsonProperty("port")]
        public string Port { get; set; }
    }
}