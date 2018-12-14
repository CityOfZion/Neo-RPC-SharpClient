using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using NeoModules.Rest.DTOs.NeoScan;
using NeoModules.Rest.Interfaces;
using Newtonsoft.Json.Linq;

namespace NeoModules.Rest.Services
{
    public enum NeoScanNet
    {
        MainNet,
        TestNet
    }

    public class NeoScanRestService : INeoscanService
    {
        private static readonly string neoScanTestNetUrl = "https://neoscan-testnet.io/api/test_net/v1/";
        private static readonly string neoScanMainNetUrl = "https://neoscan.io/api/main_net/v1/";
        private static readonly string getBalanceUrl = "get_balance/";
        private static readonly string getClaimedUrl = "get_claimed/";
        private static readonly string getClaimableUrl = "get_claimable/";
        private static readonly string getUnclaimedUrl = "get_unclaimed/";
        private static readonly string getAllNodes = "get_all_nodes/";
        private static readonly string getTransaction = "get_transaction/";
        private static readonly string getAddressAbstracts = "get_address_abstracts/";
        private static readonly string getAddressToAddressAbstracts = "get_address_to_address_abstracts/";
        private static readonly string getBlock = "get_block/";
        private static readonly string getHeight = "get_height/";
        private static readonly string getLastTransactionsByAddress = "get_last_transactions_by_address/";
        private static readonly string getNodes = "get_nodes/";

        private readonly HttpClient _restClient;

        public NeoScanRestService(NeoScanNet net)
        {
            _restClient = net == NeoScanNet.MainNet
                ? new HttpClient { BaseAddress = new Uri(neoScanMainNetUrl) }
                : new HttpClient { BaseAddress = new Uri(neoScanTestNetUrl) };
        }

        public NeoScanRestService(string url)
        {
            if (string.IsNullOrEmpty(url)) throw new ArgumentNullException(nameof(url));
            _restClient = new HttpClient { BaseAddress = new Uri(url) };
        }

        // TODO: I can refractor this more, move the 3 lines of each call to a function
        public async Task<AddressBalance> GetBalanceAsync(string address)
        {
            var composedUrl = Utils.ComposeUrl(getBalanceUrl, address);
            var result = await _restClient.GetAsync(composedUrl);
            var data = await result.Content.ReadAsStringAsync();
            return AddressBalance.FromJson(data);
        }

        public async Task<Claimable> GetClaimableAsync(string address)
        {
            var composedUrl = Utils.ComposeUrl(getClaimableUrl, address);
            var result = await _restClient.GetAsync(composedUrl);
            var data = await result.Content.ReadAsStringAsync();
            return Claimable.FromJson(data);
        }

        public async Task<Claimed> GetClaimedAsync(string address)
        {
            var composedUrl = Utils.ComposeUrl(getClaimedUrl, address);
            var result = await _restClient.GetAsync(composedUrl);
            var data = await result.Content.ReadAsStringAsync();
            return Claimed.FromJson(data);
        }

        public async Task<Unclaimed> GetUnclaimedAsync(string address)
        {
            var composedUrl = Utils.ComposeUrl(getUnclaimedUrl, address);
            var result = await _restClient.GetAsync(composedUrl);
            var data = await result.Content.ReadAsStringAsync();
            return Unclaimed.FromJson(data);
        }

        public async Task<Transaction> GetTransactionAsync(string hash)
        {
            var composedUrl = Utils.ComposeUrl(getTransaction, hash);
            var result = await _restClient.GetAsync(composedUrl);
            var data = await result.Content.ReadAsStringAsync();
            return Transaction.FromJson(data);
        }

        public async Task<List<Node>> GetAllNodesAsync()
        {
            var result = await _restClient.GetAsync(getAllNodes);
            var data = await result.Content.ReadAsStringAsync();
            return Node.FromJson(data).ToList();
        }

        public async Task<long> GetHeight()
        {
            var result = await _restClient.GetAsync(getHeight);
            var data = await result.Content.ReadAsStringAsync();
            return Convert.ToInt64(JObject.Parse(data)["height"].ToString());
        }

        public void ChangeNet(NeoScanNet net)
        {
            if (_restClient == null) return;
            switch (net)
            {
                case NeoScanNet.MainNet:
                    _restClient.BaseAddress = new Uri(neoScanMainNetUrl);
                    return;
                case NeoScanNet.TestNet:
                    _restClient.BaseAddress = new Uri(neoScanTestNetUrl);
                    return;
            }
        }

        public async Task<AbstractAddress> GetAddressAbstracts(string address, int page = 0)
        {
            var composedUrl = Utils.ComposeUrl(getAddressAbstracts, string.Concat(address, "/", page));
            var result = await _restClient.GetAsync(composedUrl);
            var data = await result.Content.ReadAsStringAsync();
            return AbstractAddress.FromJson(data);
        }

        public async Task<AbstractAddress> GetAddressToAddressAbstract(string addressfrom, string addressTo, int page = 0)
        {
            var composedUrl = Utils.ComposeUrl(getAddressToAddressAbstracts,
                string.Concat(addressfrom, "/", addressTo, "/", page));
            var result = await _restClient.GetAsync(composedUrl);
            var data = await result.Content.ReadAsStringAsync();
            return AbstractAddress.FromJson(data);
        }

        public async Task<Block> GetBlock(string blockHash)
        {
            var composedUrl = Utils.ComposeUrl(getBlock, blockHash);
            var result = await _restClient.GetAsync(composedUrl);
            var data = await result.Content.ReadAsStringAsync();
            return Block.FromJson(data);
        }

        public async Task<Block> GetBlock(int blockHeight)
        {
            var composedUrl = Utils.ComposeUrl(getBlock, blockHeight);
            var result = await _restClient.GetAsync(composedUrl);
            var data = await result.Content.ReadAsStringAsync();
            return Block.FromJson(data);
        }

        public async Task<List<Transaction>> GetLastTransactionsByAddress(string address, int page = 0)
        {
            var composedUrl = Utils.ComposeUrl(getLastTransactionsByAddress,
                string.Concat(address, "/", page));
            var result = await _restClient.GetAsync(composedUrl);
            var data = await result.Content.ReadAsStringAsync();
            return Transactions.FromJson(data).ToList();
        }

        public async Task<string> GetNodes()
        {
            var result = await _restClient.GetAsync(getNodes);
            var data = await result.Content.ReadAsStringAsync();
            return data;
        }

       
    }
}