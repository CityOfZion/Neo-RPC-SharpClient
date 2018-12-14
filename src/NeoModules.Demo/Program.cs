using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Threading.Tasks;
using NeoModules.Core;
using NeoModules.Core.KeyPair;
using NeoModules.JsonRpc.Client;
using NeoModules.NEP6;
using NeoModules.NEP6.Transactions;
using NeoModules.Rest.Services;
using NeoModules.RPC.Services;
using NeoModules.RPC;
using Newtonsoft.Json;

namespace NeoModules.Demo
{
    public class Program
    {
        private static readonly RpcClient RpcClient = new RpcClient(new Uri("https://seed0.cityofzion.io:443"));

        public static void Main(string[] args)
        {
            try
            {
                var neoApiCompleteService = SetupCompleteNeoService();

                var neoApiSimpleContractService = SetupSimpleService();
                //You can also create a custom service with only the stuff that you need by creating a class that implements(":") RpcClientWrapper like: public class CustomService : RpcClientWrapper

                var nep5ApiService = SetupNep5Service();

                BlockApiTest(neoApiCompleteService).Wait();

                TestNep5Service(nep5ApiService).Wait();

                //create rest api client
                RestClientTest().Wait();

                //nodes list from http://monitor.cityofzion.io/
                NodesListTestAsync().Wait();

                //https://n1.cityofzion.io/v1/"
                NotificationsService().Wait();

                //https://api.happynodes.f27.ventures
                HappyNodesService().Wait();

                WalletAndTransactionsTest().Wait();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"There was an exception: {ex.Message}");
            }
        }

        // Returns a NeoApiService that has all the api calls (except the nep5)
        private static NeoApiService SetupCompleteNeoService()
        {
            return new NeoApiService(RpcClient);
        }

        // Returns an instance of ContractService with only the api calls that concern contracts
        private static NeoApiContractService SetupSimpleService()
        {
            return new NeoApiContractService(RpcClient);
        }

        // Returns an instance of NeoNep5Service with the api calls that concern nep5 tokens.
        private static NeoNep5Service SetupNep5Service()
        {
            return new NeoNep5Service(RpcClient);
        }

        // Block api demonstration
        private static async Task BlockApiTest(NeoApiService service)
        {
            RPC.DTOs.Block
                genesisBlock =
                    await service.Blocks.GetBlock
                        .SendRequestAsync(
                            0); // Get genesis block by index (can pass a string with block hash as parameter too)
            string bestBlockHash = await service.Blocks.GetBestBlockHash.SendRequestAsync();
            int blockCount = await service.Blocks.GetBlockCount.SendRequestAsync();
            string blockHash = await service.Blocks.GetBlockHash.SendRequestAsync(0);
            string serializedBlock =
                await service.Blocks.GetBlockSerialized
                    .SendRequestAsync(0); // (can pass a string with block hash as parameter too)
            string blockFee = await service.Blocks.GetBlockSysFee.SendRequestAsync(0);
        }

        // Nep5 api demonstration
        private static async Task TestNep5Service(NeoNep5Service nep5Service)
        {
            var name = await nep5Service.GetName("ed07cffad18f1308db51920d99a2af60ac66a7b3", true);
            var decimals = await nep5Service.GetDecimals("ed07cffad18f1308db51920d99a2af60ac66a7b3");
            var totalsupply = await nep5Service.GetTotalSupply("ed07cffad18f1308db51920d99a2af60ac66a7b3", 8);
            var symbol = await nep5Service.GetSymbol("ed07cffad18f1308db51920d99a2af60ac66a7b3", true);
            var balance = await nep5Service.GetBalance("ed07cffad18f1308db51920d99a2af60ac66a7b3",
                "0x3640b023405b4b9c818e8387bd01f67bba04dad2", 8);

            Debug.WriteLine(
                $"Token info: \nName: {name} \nSymbol: {symbol} \nDecimals: {decimals} \nTotalSupply: {totalsupply} \nBalance: {balance}");
        }


        private static async Task RestClientTest()
        {
            var testAddress = "AJN2SZJuF7j4mvKaMYAY9N8KsyD4j1fNdf";

            var restService = new NeoScanRestService(NeoScanNet.MainNet); // service creation

            // api calls
            var getBalance = await restService.GetBalanceAsync(testAddress);
            var getClaimed = await restService.GetClaimedAsync(testAddress);
            var getClaimable = await restService.GetClaimableAsync(testAddress);
            var getUnclaimed = await restService.GetUnclaimedAsync(testAddress);
            // var getAddress = await restService.GetAddressAsync(testAddress);
            var nodes = await restService.GetAllNodesAsync();
            var transaction =
                await restService.GetTransactionAsync(
                    "610e2a4c7cdc4f311be65cee48e076871b685e13cc750397f4ee5f800da3309a");
            var height = await restService.GetHeight();
            var abstractAddress = await restService.GetAddressAbstracts("AGbj6WKPUWHze12zRyEL5sx8nGPVN6NXUn", 0);
            var addressToAddressAbstract = await restService.GetAddressToAddressAbstract(
                "AJ5UVvBoz3Nc371Zq11UV6C2maMtRBvTJK",
                "AZCcft1uYtmZXxzHPr5tY7L6M85zG7Dsrv", 0);
            var block = await restService.GetBlock("54ffd56d6a052567c5d9abae43cc0504ccb8c1efe817c2843d154590f0b572f7");
            var lastTransactionsByAddress =
                await restService.GetLastTransactionsByAddress("AGbj6WKPUWHze12zRyEL5sx8nGPVN6NXUn", 0);
        }

        // Test getting the nodes list registered on http://monitor.cityofzion.io
        private static async Task<NodeList> NodesListTestAsync()
        {
            var service = new NeoNodesListService();
            var result = await service.GetNodesList(MonitorNet.TestNet);
            var nodes = JsonConvert.DeserializeObject<NodeList>(result);
            return nodes;
        }

        private static async Task WalletAndTransactionsTest()
        {
            // Create online wallet and import account
            var walletManager = new WalletManager(new NeoScanRestService(NeoScanNet.MainNet), RpcClient);
            var importedAccount = walletManager.ImportAccount("WIF HERE (or use NEP2 bellow)", "Account_label");
            //var importedAccount = walletManager.ImportAccount("encryptedPrivateKey", "password", "Account_label");

            var neoService = new NeoApiService(RpcClient);

            //invoke test example using Phantasma smart contract
            var method = "getMailboxFromAddress"; //operation
            var contractScriptHash = "ed07cffad18f1308db51920d99a2af60ac66a7b3"; // smart contract scripthash
            var script = NEP6.Helpers.Utils.GenerateScript(contractScriptHash, method,
                new object[] { "ARcZoZPn1ReBo4LPLvkEteyLu6S2A5cvY2".ToScriptHash().ToArray() });
            var result = await neoService.Contracts.InvokeScript.SendRequestAsync(script.ToHexString());
            var content = Encoding.UTF8.GetString(result.Stack[0].Value.ToString().HexToBytes()); //the way you read a result can vary

            //call contract example using Phantasma smart contract
            var RegisterInboxOperation = "registerMailbox"; //operation
            var user = importedAccount.Address.ToArray();
            var args = new object[] { user, "neomodules" };

            var transaction = await importedAccount.TransactionManager.CallContract("ed07cffad18f1308db51920d99a2af60ac66a7b3",
                RegisterInboxOperation,     //operation
                args,                       // arguments
                null,                       // array of transfer outputs you want to attach gas or neo with the contract call (optional)
                0m,                         // network fee (optional)
                null                        // attributes (optional)
                );


            // list of transfer outputs
            var transferOutputWithNep5AndGas = new List<TransferOutput>
                {
                    new TransferOutput
                    {
                        AssetId = UInt160.Parse("a58b56b30425d3d1f8902034996fcac4168ef71d"), //  e.g. Script Hash of ASA
                        Value = BigDecimal.Parse("0.0001", byte.Parse("8")), // majority of NEP5 tokens have 8 decimals
                        ScriptHash = "AddressScriptHash to sent here".ToScriptHash(),
                    },
                    new TransferOutput
                    {
                        AssetId = NEP6.Helpers.Utils.GasToken, //GAS
                        Value = BigDecimal.Parse("0.00001", byte.Parse("8")), // GAS has 8 decimals too
                        ScriptHash = "AddressScriptHash to sent here".ToScriptHash(),
                    }
                };

            var transferOutputWithOnlyGas = new List<TransferOutput>
                {
                    new TransferOutput
                    {
                        AssetId = NEP6.Helpers.Utils.GasToken, //GAS
                        Value = BigDecimal.Parse("0.00001", byte.Parse("8")), // GAS has 8 decimals too
                        ScriptHash = "AddressScriptHash to sent here".ToScriptHash(),
                    }
                };

            // Claims unclaimed gas. Does not spent your neo to make gas claimable, you have to do it yourself!
            var claim = await importedAccount.TransactionManager.ClaimGas();

            // Transfer NEP5 and gas with fee
            var invocationTx = await importedAccount.TransactionManager.TransferNep5(null, transferOutputWithNep5AndGas, null, 0.00001m);

            // Send native assets (NEO and GAS) with fee
            var nativeTx = await importedAccount.TransactionManager.SendNativeAsset(null, transferOutputWithOnlyGas, null, 0.0001m);

            // Call contract
            var scriptHash = "a58b56b30425d3d1f8902034996fcac4168ef71d".ToScriptHash().ToArray(); // ASA e.g
            var operation = "your operation here";
            var arguments = new object[] { "arg1", "arg2", "etc" };

            // Estimate Gas consumed from contract call
            var estimateContractGasCall =
                await importedAccount.TransactionManager.EstimateGasContractInvocation(scriptHash, operation, arguments);

            // Confirm a transaction
            var confirmedTransaction =
                await importedAccount.TransactionManager.WaitForTxConfirmation(invocationTx.Hash.ToString(), 10000, 10);

        }

        private static async Task NotificationsService()
        {
            var notificationService = new NotificationsService();
            var addressNotifications = await notificationService.GetAddressNotifications("AGfGWQeM6md6RtEsTUivQNXhp8p4ytkDMR", 1, null, 13, 2691913, 500);
            var blockNotifications = await notificationService.GetBlockNotifications(10);
            var contractNotifications =
                await notificationService.GetContractNotifications("0x67a5086bac196b67d5fd20745b0dc9db4d2930ed");
            var tokenList = await notificationService.GetTokens();
            var transaction =
                await notificationService.GetTransactionNotifications(
                    "1db9ad15febbf7b9cfa11603ecbe52aad5be6137a9e1d29e83465fa664c2a6ed");
        }

        private static async Task HappyNodesService()
        {
            var happyNodesService = new HappyNodesService();
            var bestBlock = await happyNodesService.GetBestBlock();
            var lastBlock = await happyNodesService.GetLastBlock();
            var blockTime = await happyNodesService.GetBlockTime();

            var unconfirmedTxs = await happyNodesService.GetUnconfirmed();

            var nodesFlat = await happyNodesService.GetNodesFlat();
            var nodes = await happyNodesService.GetNodes();
            var nodeById = await happyNodesService.GetNodeById(482);
            var edges = await happyNodesService.GetEdges();
            var nodesList = await happyNodesService.GetNodesList();

            var dailyHistory = await happyNodesService.GetDailyNodeHistory();
            var weeklyHistory = await happyNodesService.GetWeeklyNodeHistory();

            var dailyStability = await happyNodesService.GetDailyNodeStability(480);
            var weeklyStability = await happyNodesService.GetWeeklyNodeStability(0);

            var dailyLatency = await happyNodesService.GetDailyNodeLatency(480);
            var weeklyLatency = await happyNodesService.GetWeeklyNodeLatency(480);
            var blockHeightLag = await happyNodesService.GetNodeBlockheightLag(0);
            var endpoints = await happyNodesService.GetEndPoints();
        }
    }
}