using NeoModules.NEP6;
using NeoModules.Rest.Services;
using System;
using System.Diagnostics;
using System.Threading.Tasks;
using NeoModules.Core;
using NeoModules.JsonRpc.Client;
using NeoModules.NEP6.TransactionManagers;
using NeoModules.NEP6.Transactions;
using NeoModules.Rest.DTOs.Switcheo;
using NeoModules.Rest.Helpers;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace NeoModules.SwitcheoDemo
{
    public class Program
    {
        private static readonly RpcClient RpcTestNetClient = new RpcClient(new Uri("http://test5.cityofzion.io:8880"));

        private static SwitcheoRestService _switcheoRestService;

        public static void Main(string[] args)
        {
            try
            {
                SetupSwitheoService().Wait();   // setup the service
                NoAuthCallsDemo().Wait();       // demo for calls that do not need authentication
                AuthCallsDemo().Wait();         // demo for calls that do need authentication
            }
            catch (Exception ex)
            {
                Console.WriteLine($"There was an exception: {ex.Message}");
            }
        }

        public static async Task SetupSwitheoService()
        {
            _switcheoRestService = new SwitcheoRestService(SwitcheoNet.TestNet);
            await _switcheoRestService.InitService("neo");
        }

        public static async Task AuthCallsDemo()
        {
            var switcheoService = new SwitcheoRestService(SwitcheoNet.TestNet);
            await switcheoService.InitService("neo");

            var walletManager = new WalletManager(new NeoScanRestService(NeoScanNet.TestNet), RpcTestNetClient);

            // test account
            var importedAccount = walletManager.ImportAccount("L3jBjdWz8epSg6NxVkefuficuLKCCEvxFTce9NXfQ6kyeceqdR1g", "Switcheo");

            if (importedAccount.TransactionManager is AccountSignerTransactionManager accountSignerTransactionManager)
            {
                await DepositDemo(switcheoService, accountSignerTransactionManager);

                await WithdrawalDemo(switcheoService, accountSignerTransactionManager);

                await OrderDemo(switcheoService, accountSignerTransactionManager);

                await CancelOrderDemo(switcheoService, accountSignerTransactionManager);
            }
        }

        public static async Task NoAuthCallsDemo()
        {
            var timeStamp = await _switcheoRestService.GetTimeStampAsync();
            var contracts = await _switcheoRestService.GetContractsAsync();
            var tokens = await _switcheoRestService.GetTokensAsync();
            var pairs = await _switcheoRestService.GetPairsAsync();
            var candleSticks = await _switcheoRestService.GetCandleSticksAsync("SWTH_NEO", 1539541508, 1539541808, 60);
            var last24HourData = await _switcheoRestService.Get24HourDataAsync();
            var lastPrice = await _switcheoRestService.GetLastPriceAsync(null);
            var offers = await _switcheoRestService.GetOffers("SWTH_NEO");
            var trades = await _switcheoRestService.GetTradesAsync("SWTH_NEO");
            var recentTrades = await _switcheoRestService.GetRecentTradesAsync("SWTH_NEO");
            var listBalances = await _switcheoRestService.ListBalances(new[] { "c20003921d7b20335f26c8370bcb6157739eee6b" }, new[] { "a195c1549e7da61b8da315765a790ac7e7633b82" });

            Debug.WriteLine(timeStamp);
            Debug.WriteLine(JsonConvert.SerializeObject(contracts));
            Debug.WriteLine(JsonConvert.SerializeObject(tokens));
            Debug.WriteLine(JsonConvert.SerializeObject(pairs));
            Debug.WriteLine(JsonConvert.SerializeObject(candleSticks));
            Debug.WriteLine(JsonConvert.SerializeObject(last24HourData));
            Debug.WriteLine(JsonConvert.SerializeObject(lastPrice));
            Debug.WriteLine(JsonConvert.SerializeObject(offers));
            Debug.WriteLine(JsonConvert.SerializeObject(trades));
            Debug.WriteLine(JsonConvert.SerializeObject(recentTrades));
            Debug.WriteLine(JsonConvert.SerializeObject(listBalances));
        }

        public static async Task DepositDemo(SwitcheoRestService switcheoService, AccountSignerTransactionManager accountSigner)
        {
            // create deposit DTO
            var transactObject = await switcheoService.PrepareCreateDeposit("SWTH", "10");
            // turn into json
            var signableParams = JsonConvert.SerializeObject(transactObject);
            // serialize request params
            var serializedParams = SwitcheoHelper.PrepareParametersRequest(signableParams);
            // signs the serialized params
            var signature = accountSigner.SignMessage(serializedParams);

            // adds the 'address' and 'signature' fields to the json
            var apiParams = SwitcheoHelper.AddTransactFields(signableParams, signature,
                accountSigner.AddressScriptHash.ToString().Remove(0, 2));

            // sends the 'create deposit' request
            CreateResponse response = await switcheoService.CreateDeposit(apiParams);
            // check response to make sure is what you want
            Debug.WriteLine(JsonConvert.SerializeObject(response));

            // execute deposit
            var tx = Transaction.FromJson(Txn.ToJson(response.Transaction));
            var depositId = response.Id.ToString();
            var signatureTx = accountSigner.SignTransaction(tx, false).ToHexString();

            // if everything is good it should return OK
            var execute = await switcheoService.ExecuteDeposit(signatureTx, depositId);
            Debug.WriteLine(execute);
        }

        public static async Task WithdrawalDemo(SwitcheoRestService switcheoService, AccountSignerTransactionManager accountSigner)
        {
            // create withdrawal DTO
            var transactObject = await switcheoService.PrepareCreateWithdrawal("neo", "SWTH", "3");
            // turn into json
            var signableParams = JsonConvert.SerializeObject(transactObject);
            // serialize request params
            var serializedParams = SwitcheoHelper.PrepareParametersRequest(signableParams);
            // signs the serialized params
            var signature = accountSigner.SignMessage(serializedParams);

            // adds the 'address' and 'signature' fields to the json
            var apiParams = SwitcheoHelper.AddTransactFields(signableParams, signature,
                accountSigner.AddressScriptHash.ToString().Remove(0, 2));

            // sends the 'create withdrawal' request
            var response = await switcheoService.CreateWithdrawl(apiParams);
            Debug.WriteLine(JsonConvert.SerializeObject(response));

            // execute withdrawal
            JObject createWithdrawalResponse = JObject.Parse(response);
            createWithdrawalResponse.Remove("transaction"); // not in docs

            var executeWithdrawalDto = new ExecuteWithdrawl
            {
                Id = new Guid(createWithdrawalResponse["id"].ToString()),
                Timestamp = await switcheoService.GetTimeStampAsync()
            };

            // serialize request params
            var serializedResponseParams = SwitcheoHelper.PrepareParametersRequest(JsonConvert.SerializeObject(executeWithdrawalDto));
            var responseSignature = accountSigner.SignMessage(serializedResponseParams);

            WithdrawlResponse withdrawlResponse = await switcheoService.ExecuteWithdraw(executeWithdrawalDto, responseSignature);
            Debug.WriteLine(JsonConvert.SerializeObject(withdrawlResponse));
        }

        public static async Task OrderDemo(SwitcheoRestService switcheoService, AccountSignerTransactionManager accountSigner)
        {
            var orderRequest = await switcheoService.PrepareCreateOrder("SWTH_NEO", "buy", "0.00315200", "2050000000", true, "limit");

            // turn into json
            var signableParams = JsonConvert.SerializeObject(orderRequest);
            // serialize request params
            var serializedParams = SwitcheoHelper.PrepareParametersRequest(signableParams);
            // signs the serialized params
            var signature = accountSigner.SignMessage(serializedParams);

            // adds the 'address' and 'signature' fields to the json
            var apiParams = SwitcheoHelper.AddTransactFields(signableParams, signature,
                accountSigner.AddressScriptHash.ToString().Remove(0, 2));

            // sends the 'create withdrawal' request
            OrderResponse response = await switcheoService.CreateOrder(apiParams);
            // check response to make sure is what you want
            Debug.WriteLine(JsonConvert.SerializeObject(response));

            var executeOrderDto = new ExecuteOrder();
            foreach (var responseFill in response.Fills)
            {
                var tx = Transaction.FromJson(JsonConvert.SerializeObject(responseFill.Txn));
                var signatureTx = accountSigner.SignTransaction(tx, false).ToHexString();
                executeOrderDto.Fills.Add(responseFill.Id.ToString(), signatureTx);
            }

            foreach (var responseMakes in response.Makes)
            {
                var tx = Transaction.FromJson(JsonConvert.SerializeObject(responseMakes.Txn));
                var signatureTx = accountSigner.SignTransaction(tx, false).ToHexString();
                executeOrderDto.Makes.Add(responseMakes.Id.ToString(), signatureTx);
            }

            ExecuteOrderResponse execute = await switcheoService.ExecuteOrder(executeOrderDto, response.Id.ToString());
            Debug.WriteLine(JsonConvert.SerializeObject(execute));
        }

        public static async Task CancelOrderDemo(SwitcheoRestService switcheoService,
            AccountSignerTransactionManager accountSigner)
        {
            var cancelRequest = await switcheoService.PrepareCancelOrder("69c60da5-5832-4705-8390-de4bb4ed62c5");

            // turn into json
            var signableParams = JsonConvert.SerializeObject(cancelRequest);
            // serialize request params
            var serializedParams = SwitcheoHelper.PrepareParametersRequest(signableParams);
            // signs the serialized params
            var signature = accountSigner.SignMessage(serializedParams);

            // adds the 'address' and 'signature' fields to the json
            var apiParams = SwitcheoHelper.AddTransactFields(signableParams, signature,
                accountSigner.AddressScriptHash.ToString().Remove(0, 2));

            // sends the 'create cancellation' request
            CreateResponse response = await switcheoService.CreateCancellation(apiParams);
            Debug.WriteLine(JsonConvert.SerializeObject(response));
            // check response to make sure is what you want

            // execute cancellation
            var tx = Transaction.FromJson(Txn.ToJson(response.Transaction));
            var depositId = response.Id.ToString();
            var signatureTx = accountSigner.SignTransaction(tx, false).ToHexString();

            // if everything is good it should return OK
            var execute = await switcheoService.ExecuteCancellation(signatureTx, depositId);
            Debug.WriteLine(JsonConvert.SerializeObject(execute));
        }
    }
}
