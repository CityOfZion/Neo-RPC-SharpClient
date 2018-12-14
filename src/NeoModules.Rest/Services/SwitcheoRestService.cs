using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using NeoModules.Core;
using NeoModules.Rest.DTOs.Switcheo;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace NeoModules.Rest.Services
{
    public class SwitcheoRestService
    {
        private readonly HttpClient _restClient;

        public string ContractHash { get; set; }
        public string Blockchain { get; set; }

        public SwitcheoRestService(SwitcheoNet net)
        {
            _restClient = net == SwitcheoNet.MainNet
                ? new HttpClient { BaseAddress = new Uri(neoScanMainNetUrl) }
                : new HttpClient { BaseAddress = new Uri(switcheoTestNetUrl) };
        }

        /// <summary>
        /// Gets the last contract hash for the specified blockchain
        /// Sets the Blockchain and ContractHash fields for future calls
        /// <param name="blockchain"></param>
        /// </summary>
        public async Task InitService(string blockchain)
        {
            var contracts = await GetContractsAsync();
            var lastContractHash = contracts[blockchain.ToUpperInvariant()].Last.ToObject<string>();
            if (!string.IsNullOrEmpty(lastContractHash))
            {
                ContractHash = lastContractHash;
                Blockchain = blockchain.ToLowerInvariant();
            }
        }

        /// <summary>
        /// Retrieve the current timestamp in the exchange, this value should be fetched and used when a timestamp parameter is required for API requests.
        /// If the timestamp used for your API request is not within an acceptable range of the exchange's timestamp then an invalid signature error will be returned.
        /// The acceptable range might vary, but it should be less than one minute.
        /// </summary>
        /// <returns></returns>
        public async Task<long> GetTimeStampAsync()
        {
            var result = await ExecuteCall(getTimestamp);
            var data = (string)JObject.Parse(result)["timestamp"];
            return long.Parse(data);
        }

        /// <summary>
        /// Retrieve updated contract hashes deployed by Switcheo.
        /// Please note that different contract hashes should be used for the TestNet vs the MainNet.
        /// </summary>
        /// <returns></returns>
        public async Task<JObject> GetContractsAsync() // returns a JObject because the json it does not have a specific format
        {
            var result = await ExecuteCall(getContracts);
            return JObject.Parse(result);
        }

        /// <summary>
        /// Retrieve a list of supported tokens on Switcheo.
        /// </summary>
        /// <returns></returns>
        public async Task<Dictionary<string, TokenData>> GetTokensAsync()
        {
            var result = await ExecuteCall(getTokens);
            return TokenData.FromJson(result);
        }

        /// <summary>
        /// Retrieve available currency pairs on Switcheo Exchange filtered by the base parameter. Defaults to all pairs.
        /// The valid base currencies are currently: NEO, GAS, SWTH.
        /// </summary>
        /// <returns></returns>
        public async Task<List<string>> GetPairsAsync()
        {
            var result = await ExecuteCall(getPairs);
            return JsonConvert.DeserializeObject<List<string>>(result);
        }

        /// <summary>
        /// Returns candlestick chart data filtered by url parameters.
        /// </summary>
        /// <param name="pair"></param>
        /// <param name="startTime">Start of time range for data in epoch seconds</param>
        /// <param name="endTime">End of time range for data in epoch seconds</param>
        /// <param name="interval">Candlestick period in minutes Possible values are: 1, 5, 30, 60, 360, 1440</param>
        /// <returns></returns>
        public async Task<List<CandleStick>> GetCandleSticksAsync(string pair, int startTime, int endTime, int interval)
        {
            if (string.IsNullOrEmpty(pair)) throw new ArgumentNullException(nameof(pair));
            if (startTime <= 0) throw new ArgumentOutOfRangeException(nameof(startTime));
            if (endTime <= 0) throw new ArgumentOutOfRangeException(nameof(endTime));
            if (interval <= 0) throw new ArgumentOutOfRangeException(nameof(interval));

            var query = HttpUtility.ParseQueryString(string.Empty);
            query["pair"] = pair;
            query["interval"] = interval.ToString();
            query["start_time"] = startTime.ToString();
            query["end_time"] = endTime.ToString();
            string queryString = query.ToString().Insert(0, "?");
            var result = await ExecuteCall(Utils.ComposeUrl(getCandleSticks, queryString));
            return CandleStick.FromJson(result);
        }

        /// <summary>
        /// Returns 24-hour data for all pairs and markets.
        /// </summary>
        /// <returns></returns>
        public async Task<List<CandleStick>> Get24HourDataAsync()
        {
            var result = await ExecuteCall(get24HourData);
            return CandleStick.FromJson(result);
        }

        /// <summary>
        /// Returns last price of given symbol(s). Defaults to all symbols.
        /// </summary>
        /// <param name="symbols"></param>
        /// <returns></returns>
        public async Task<Dictionary<string, LastPrice>> GetLastPriceAsync(List<string> symbols)
        //todo ask for how to create query of array, did not work with "?symbols=GAS&symbols=NEO"
        {
            // ignore param for now
            //    var query = HttpUtility.ParseQueryString(string.Empty);
            //    foreach (var symbol in symbols)
            //    {
            //        query["symbols"] = symbol;
            //    }

            var result = await ExecuteCall(getLastPrice);
            return LastPrice.FromJson(result);
        }

        /// <summary>
        /// Retrieves the best 70 offers (per side) on the offer book.
        /// </summary>
        /// <param name="blockchain"></param>
        /// <param name="pair"></param>
        /// <param name="contractHash"></param>
        /// <returns></returns>
        public async Task<List<Offers>> GetOffers(string pair, string blockchain = null, string contractHash = null)
        {
            if (string.IsNullOrEmpty(blockchain)) blockchain = Blockchain;
            if (string.IsNullOrEmpty(contractHash)) contractHash = ContractHash;
            if (string.IsNullOrEmpty(pair)) throw new ArgumentNullException(nameof(pair));

            var query = HttpUtility.ParseQueryString(string.Empty);
            query["blockchain"] = blockchain;
            query["pair"] = pair;
            query["contract_hash"] = contractHash;
            string queryString = query.ToString().Insert(0, "?");
            var result = await ExecuteCall(Utils.ComposeUrl(getOffers, queryString));
            return Offers.FromJson(result);
        }

        /// <summary>
        /// Retrieves trades that have already occurred on Switcheo Exchange filtered by the request parameters.
        /// </summary>
        /// <param name="contractHash"></param>
        /// <param name="pair"></param>
        /// <param name="from"></param>
        /// <param name="to"></param>
        /// <param name="limit"></param>
        /// <returns></returns>
        public async Task<List<Trade>> GetTradesAsync(string pair, string contractHash = null, int from = -1, int to = -1, int limit = -1)
        {
            if (string.IsNullOrEmpty(contractHash)) contractHash = ContractHash;

            var query = HttpUtility.ParseQueryString(string.Empty);
            query["pair"] = pair;
            query["contract_hash"] = contractHash;
            if (from != -1) query["from"] = from.ToString();
            if (to != -1) query["to"] = to.ToString();
            if (limit != -1) query["limit"] = limit.ToString();
            string queryString = query.ToString().Insert(0, "?");
            var result = await ExecuteCall(Utils.ComposeUrl(getTrades, queryString));
            return Trade.FromJson(result);
        }

        /// <summary>
        /// Returns 20 most recent trades on all pairs sorted in descending order by executed time.
        /// </summary>
        /// <param name="pair"></param>
        /// <returns></returns>
        public async Task<List<RecentTrade>> GetRecentTradesAsync(string pair)
        {
            if (string.IsNullOrEmpty(pair)) throw new ArgumentNullException(nameof(pair));
            var query = HttpUtility.ParseQueryString(string.Empty);
            query["pair"] = pair;
            string queryString = query.ToString().Insert(0, "?");
            var result = await ExecuteCall(Utils.ComposeUrl(getRecentTrades, queryString));
            return RecentTrade.FromJson(result);
        }

        /// <summary>
        /// Creates a Transact object used for CreateDeposit call
        /// </summary>
        /// <param name="blockchain"></param>
        /// <param name="contractHash"></param>
        /// <param name="assetId"></param>
        /// <param name="amount"></param>
        /// <returns></returns>
        public async Task<Transact> PrepareCreateDeposit(string assetId, string amount, string blockchain = null, string contractHash = null)
        {
            if (string.IsNullOrEmpty(assetId)) throw new ArgumentNullException(nameof(assetId));
            if (string.IsNullOrEmpty(amount)) throw new ArgumentNullException(nameof(amount));
            if (string.IsNullOrEmpty(contractHash)) contractHash = ContractHash;
            if (string.IsNullOrEmpty(blockchain)) blockchain = Blockchain;

            var timestamp = await GetTimeStampAsync();
            var tokens = await GetTokensAsync();
            int decimals;
            if (tokens.ContainsKey(assetId))
            {
                decimals = tokens[assetId].Decimals;
            }
            else
            {
                throw new SwitcheoException($"{assetId} is not available for deposit in Switcheo");
            }
            var transact = new Transact
            {
                Blockchain = blockchain,
                AssetId = assetId,
                Timestamp = timestamp,
                ContractHash = contractHash,
                Amount = (long)BigDecimal.Parse(amount, byte.Parse(decimals.ToString())).Value
            };

            return transact;
        }

        /// <summary>
        /// This endpoint creates a deposit which can be executed through Execute Deposit.
        /// To be able to make a deposit, sufficient funds are required in the depositing wallet.
        /// IMPORTANT: After calling this endpoint, the Execute Deposit endpoint has to be called for the deposit to be executed. 
        /// </summary>
        /// <param name="apiParams"></param>
        /// <returns></returns>
        public async Task<CreateResponse> CreateDeposit(string apiParams)
        {
            var httpContent = new StringContent(apiParams, Encoding.UTF8, "application/json");
            var result = await ExecuteCall(createDeposit, httpContent);
            return CreateResponse.FromJson(result);
        }

        /// <summary>
        /// This is the second endpoint required to execute a deposit.
        /// After using the Create Deposit endpoint, you will receive a response which requires additional signing.
        /// The signature should then be attached as the signature parameter in the request payload.
        /// </summary>
        /// <param name="signature"></param>
        /// <param name="id"></param>
        /// <returns></returns>
        public async Task<string> ExecuteDeposit(string signature, string id)
        {
            var json = new JObject { ["signature"] = signature };
            var httpContent = new StringContent(json.ToString(), Encoding.UTF8, "application/json");
            var result = await ExecuteCall(executeDeposit.Replace(":id", id), httpContent);
            return result;
        }

        /// <summary>
        /// Creates a Transact object used for CreateWithdrawal call
        /// </summary>
        /// <param name="blockchain"></param>
        /// <param name="contractHash"></param>
        /// <param name="assetId"></param>
        /// <param name="amount"></param>
        /// <returns></returns>
        public async Task<Transact> PrepareCreateWithdrawal(string assetId, string amount, string blockchain = null, string contractHash = null)
        {
            if (string.IsNullOrEmpty(assetId)) throw new ArgumentNullException(nameof(assetId));
            if (string.IsNullOrEmpty(amount)) throw new ArgumentNullException(nameof(amount));
            if (string.IsNullOrEmpty(contractHash)) contractHash = ContractHash;
            if (string.IsNullOrEmpty(blockchain)) blockchain = Blockchain;

            var timestamp = await GetTimeStampAsync();
            var tokens = await GetTokensAsync();
            int decimals;
            if (tokens.ContainsKey(assetId))
            {
                decimals = tokens[assetId].Decimals;
            }
            else
            {
                throw new Exception($"{assetId} is not available for withdrawal in Switcheo");
            }
            var transact = new Transact
            {
                Blockchain = blockchain,
                AssetId = assetId,
                Timestamp = timestamp,
                ContractHash = contractHash,
                Amount = (long)BigDecimal.Parse(amount, byte.Parse(decimals.ToString())).Value
            };

            return transact;
        }

        /// <summary>
        /// This endpoint creates a withdrawal which can be executed through Execute Withdrawal.
        /// To be able to make a withdrawal, sufficient funds are required in the contract balance.
        /// A signature of the request payload has to be provided for this API call.
        /// </summary>
        /// <param name="apiParams"></param>
        /// <returns></returns>
        public async Task<string> CreateWithdrawl(string apiParams)
        {
            var httpContent = new StringContent(apiParams, Encoding.UTF8, "application/json");
            var result = await ExecuteCall(createWithdrawl, httpContent);
            return result;
        }

        /// <summary>
        /// This is the second endpoint required to execute a withdrawal.
        /// After using the Create Withdrawal endpoint, you will receive a response which requires additional signing.
        /// </summary>
        /// <param name="withdrawal"></param>
        /// <param name="signature"></param>
        /// <returns></returns>
        public async Task<WithdrawlResponse> ExecuteWithdraw(ExecuteWithdrawl withdrawal, string signature)
        {
            var apiParams = new JObject
            {
                ["id"] = withdrawal.Id,
                ["timestamp"] = withdrawal.Timestamp,
                ["signature"] = signature
            };

            var httpContent = new StringContent(apiParams.ToString(), Encoding.UTF8, "application/json");
            var result = await ExecuteCall(executeWithdrawl.Replace(":id", withdrawal.Id.ToString()), httpContent);
            return WithdrawlResponse.FromJson(result);
        }

        /// <summary>
        /// Retrieves orders from a specific address filtered by the given parameters.
        /// </summary>
        /// <param name="address"></param>
        /// <param name="contractHash"></param>
        /// <param name="pair"></param>
        /// <param name="from"></param>
        /// <param name="orderStatus"></param>
        /// <param name="beforeId"></param>
        /// <param name="limit"></param>
        /// <returns></returns>
        public async Task<OrderResponse> GetOrders(string address, string contractHash = null, string pair = null,
            int from = -1, string orderStatus = null, string beforeId = null, int limit = -1)
        {
            if (string.IsNullOrEmpty(contractHash)) contractHash = ContractHash;

            var query = HttpUtility.ParseQueryString(string.Empty);
            query["address"] = address;
            query["contract_hash"] = contractHash;

            //optional args
            if (!string.IsNullOrEmpty(pair)) query["pair"] = pair;
            if (!string.IsNullOrEmpty(orderStatus)) query["orderStatus"] = orderStatus;
            if (!string.IsNullOrEmpty(beforeId)) query["beforeId"] = beforeId;
            if (from >= 0) query["from"] = from.ToString();
            if (limit >= 0) query["limit"] = limit.ToString();

            string queryString = query.ToString().Insert(0, "?");
            var result = await ExecuteCall(Utils.ComposeUrl(orders, queryString));
            return OrderResponse.FromJson(result);
        }

        /// <summary>
        /// Prepares a OrderRequest object
        /// </summary>
        /// <param name="pair"></param>
        /// <param name="side"></param>
        /// <param name="price"></param>
        /// <param name="wantAmount"></param>
        /// <param name="useNativeTokens"></param>
        /// <param name="orderType"></param>
        /// <param name="blockchain"></param>
        /// <param name="contractHash"></param>
        /// <returns></returns>
        public async Task<OrderRequest> PrepareCreateOrder(string pair, string side, string price, string wantAmount, bool useNativeTokens,
            string orderType, string blockchain = null, string contractHash = null)
        {
            if (string.IsNullOrEmpty(pair)) throw new ArgumentNullException(nameof(pair));
            if (string.IsNullOrEmpty(side)) throw new ArgumentNullException(nameof(side));
            if (string.IsNullOrEmpty(price)) throw new ArgumentNullException(nameof(price));
            if (string.IsNullOrEmpty(wantAmount)) throw new ArgumentNullException(nameof(wantAmount));
            if (string.IsNullOrEmpty(orderType)) throw new ArgumentNullException(nameof(orderType));
            //if (string.IsNullOrEmpty(otcAddress)) throw new ArgumentNullException(nameof(otcAddress));
            if (string.IsNullOrEmpty(contractHash)) contractHash = ContractHash;
            if (string.IsNullOrEmpty(blockchain)) blockchain = Blockchain;

            var timestamp = await GetTimeStampAsync();
            var order = new OrderRequest
            {
                Blockchain = blockchain,
                ContractHash = contractHash,
                OrderType = orderType,
                Pair = pair,
                Price = price,
                Side = side,
                Timestamp = timestamp,
                UseNativeTokens = useNativeTokens,
                WantAmount = wantAmount
            };
            return order;
        }

        /// <summary>
        /// This endpoint creates an order which can be executed through Broadcast Order.
        /// Orders can only be created after sufficient funds have been deposited into the user's contract balance.
        /// A successful order will have zero or one make and/or zero or more fills.
        /// </summary>
        /// <param name="apiParams"></param>
        /// <returns></returns>
        public async Task<OrderResponse> CreateOrder(string apiParams)
        {
            var httpContent = new StringContent(apiParams, Encoding.UTF8, "application/json");
            var result = await ExecuteCall(orders, httpContent);
            return OrderResponse.FromJson(result);
        }

        /// <summary>
        /// This is the second endpoint required to execute an order.
        /// After using the Create Order endpoint, you will receive a response which needs to be signed.
        /// </summary>
        /// <param name="order"></param>
        /// <param name="id"></param>
        /// <returns></returns>
        public async Task<ExecuteOrderResponse> ExecuteOrder(ExecuteOrder order, string id)
        {
            var json = JsonConvert.SerializeObject(order);
            var signaturesJson = $"{{\"signatures\":{json}}}";
            var httpContent = new StringContent(signaturesJson, Encoding.UTF8, "application/json");
            var result = await ExecuteCall(executeOrder.Replace(":id", id), httpContent);
            return ExecuteOrderResponse.FromJson(result);
        }

        /// <summary>
        /// Prepares a CancelOrderRequest object
        /// </summary>
        /// <param name="orderId"></param>
        /// <returns></returns>
        public async Task<CancelOrderRequest> PrepareCancelOrder(string orderId)
        {
            if (string.IsNullOrEmpty(orderId)) throw new ArgumentNullException(nameof(orderId));
            var timestamp = await GetTimeStampAsync();
            var cancelOrderRequest = new CancelOrderRequest
            {
                OrderId = Guid.Parse(orderId),
                Timestamp = timestamp
            };
            return cancelOrderRequest;
        }

        /// <summary>
        /// This is the first API call required to cancel an order.
        /// Only orders with makes and with an available_amount of more than 0 can be cancelled.
        /// </summary>
        /// <param name="apiParams"></param>
        /// <returns></returns>
        public async Task<CreateResponse> CreateCancellation(string apiParams)
        {
            var httpContent = new StringContent(apiParams, Encoding.UTF8, "application/json");
            var result = await ExecuteCall(cancellationRequest, httpContent);
            return CreateResponse.FromJson(result);
        }

        /// <summary>
        /// This is the second endpoint that must be called to cancel an order.
        /// After calling the Create Cancellation endpoint, you will receive a transaction in the response which must be signed.
        /// </summary>
        /// <param name="signature"></param>
        /// <param name="id"></param>
        /// <returns></returns>
        public async Task<string> ExecuteCancellation(string signature, string id)
        {
            var json = new JObject { ["signature"] = signature };
            var httpContent = new StringContent(json.ToString(), Encoding.UTF8, "application/json");
            var result = await ExecuteCall(executeCancellation.Replace(":id", id), httpContent);
            return result;
        }

        /// <summary>
        /// List contract balances of the given address and contract.
        /// The purpose of this endpoint is to allow convenient querying of a user's balance across multiple blockchains, for example, if you want to retrieve a user's NEO and ethereum balances.
        /// </summary>
        /// <param name="addresses"></param>
        /// <param name="contractsHash"></param>
        /// <returns></returns>
        public async Task<string> ListBalances(string[] addresses, string[] contractsHash)
        {
            var url = addresses.Aggregate("?", (current, address) => current + $"addresses[]={address}&");
            url = contractsHash.Aggregate(url, (current, contractHash) => current + $"contract_hashes[]={contractHash}&");

            if (url[url.Length - 1] == '&')
            {
                url = url.Remove(url.Length - 1);
            }

            var result = await ExecuteCall(Utils.ComposeUrl(getBalances, url));
            return result;
        }

        private async Task<string> ExecuteCall(string parameters, HttpContent content = null)
        {
            HttpResponseMessage result;
            if (content == null) // GET
            {
                result = await _restClient.GetAsync(parameters);
            }
            else // POST
            {
                result = await _restClient.PostAsync(parameters, content);
            }

            if (result.StatusCode != HttpStatusCode.OK)
            {
                var statusCode = (int)result.StatusCode;
                if (SwitcheoErrors.ContainsKey(statusCode))
                {
                    throw new SwitcheoException(SwitcheoErrors[statusCode]);
                }
                throw new Exception(result.StatusCode.ToString());
            }

            return await result.Content.ReadAsStringAsync();
        }

        #region URLs

        private static readonly string switcheoTestNetUrl = "https://test-api.switcheo.network/v2/";
        private static readonly string neoScanMainNetUrl = "https://api.switcheo.network/v2/";

        private const string getTimestamp = "exchange/timestamp";
        private const string getContracts = "exchange/contracts";
        private const string getTokens = "exchange/tokens";
        private const string getPairs = "exchange/pairs";
        private const string getCandleSticks = "tickers/candlesticks";
        private const string get24HourData = "tickers/last_24_hours";
        private const string getLastPrice = "tickers/last_price";
        private const string getOffers = "offers";
        private const string getTrades = "trades";
        private const string getRecentTrades = "trades/recent";
        private const string getBalances = "balances";
        private const string createDeposit = "deposits";
        private const string executeDeposit = "deposits/:id/broadcast";
        private const string createWithdrawl = "withdrawals";
        private const string executeWithdrawl = "withdrawals/:id/broadcast";
        private const string orders = "orders";
        private const string executeOrder = "orders/:id/broadcast";
        private const string cancellationRequest = "cancellations";
        private const string executeCancellation = "cancellations/:id/broadcast";

        #endregion

        private static readonly Dictionary<int, string> SwitcheoErrors = new Dictionary<int, string>
        {
            {400,"Bad Request -- Your request is badly formed."},
            {401,"Unauthorized -- You did not provide a valid signature."},
            {404,"Not Found -- The specified endpoint or resource could not be found."},
            {406,"Not Acceptable -- You requested a format that isn't json."},
            {409,"Too Many Requests -- Slow down requests and use Exponential backoff timing."},
            {422,"Unprocessible Entity -- Your request had validation errors."},
            {500,"Internal Server Error -- We had a problem with our server. Try again later."},
            {503,"Service Unavailable -- We're temporarily offline for maintenance. Please try again later."}
        };
    }

    public enum SwitcheoNet
    {
        MainNet,
        TestNet
    }

    [Serializable]
    public class SwitcheoException : Exception
    {
        public SwitcheoException(string message) : base(message) { }
    }
}
