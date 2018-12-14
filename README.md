<p align="center">
  <img
    src="http://res.cloudinary.com/vidsy/image/upload/v1503160820/CoZ_Icon_DARKBLUE_200x178px_oq0gxm.png"
    width="125px;">
</p>

<h1 align="center">NeoModules</h1>

<p align="center">
  Set of tools for C# devs to use on your <b>NEO</b> blockchain project.
</p>

<p align="center">
  <b>Waiting for peer review. Use on main net at your own risk</b>
</p>


---

Need info? [Create an issue](https://github.com/CityOfZion/NeoModules/issues/new)
 | [Ask me on CoZ Discord - Bruno Freitas](https://discordapp.com/invite/R8v48YA) |

## Overview

|  Project Source | Nuget Package |  Description |
| ------------- |--------------------------|-----------|
| [NeoModules.RPC](https://github.com/CityOfZion/NeoModules/tree/master/src/NeoModules.RPC)    | [![NuGet version](https://img.shields.io/badge/nuget-1.2-green.svg)](https://www.nuget.org/packages/NeoModules.RPC/)| RPC Class Library to interact with NEO RPC nodes |
| [NeoModules.JsonRpc.Client](https://github.com/CityOfZion/NeoModules/tree/master/src/NeoModules.JsonRpc.Client) | [![NuGet version](https://img.shields.io/badge/nuget-1.0.2-green.svg)](https://www.nuget.org/packages/NeoModules.JsonRpc.Client/)| Base RPC client definition, used in NeoModules.RPC|
| [NeoModules.Rest](https://github.com/CityOfZion/NeoModules/tree/master/src/NeoModules.Rest)    | [![NuGet version](https://img.shields.io/badge/nuget-2.0-green.svg)](https://www.nuget.org/packages/NeoModules.Rest/)| REST clients for Neoscan, Notifications, HappyNodes and Switcheo |
| [NeoModules.Core](https://github.com/CityOfZion/NeoModules/tree/feature-core/src/NeoModules.Core)    | [![NuGet version](https://img.shields.io/badge/nuget-1.0-green.svg)](https://www.nuget.org/packages/NeoModules.Core/)| Core data types and methods used in NeoModules |
| [NeoModules.NEP6](https://github.com/CityOfZion/NeoModules/tree/feature-core/src/NeoModules.NEP6)    | [![NuGet version](https://img.shields.io/badge/nuget-0.1.1-green.svg)](https://www.nuget.org/packages/NeoModules.NEP6/)| NEP6 light wallet implementation |

## Table of Contents

- [Getting Started](https://github.com/CityOfZion/NeoModules#getting-started)
  - [Prerequisites](https://github.com/CityOfZion/NeoModules#prerequisites)
  - [Installing](https://github.com/CityOfZion/NeoModules#installing)
- [RPC client](https://github.com/CityOfZion/NeoModules#RPC-client)
- [Rest services](https://github.com/CityOfZion/NeoModules#Rest-services)
- [NEP6 Compatible Wallet](https://github.com/CityOfZion/NeoModules#NEP6-Compatible-Wallet)
- [Authors](https://github.com/CityOfZion/NeoModules#authors)
- [License](https://github.com/CityOfZion/NeoModules#license)

## Getting Started

These instructions will get you a copy of the project up and running on your machine for NEO apps related development and testing purposes.

### Prerequisites

You only need Visual Studio, I recommend latest version.

### Installing

NeoModules is available via NuGet:

```
PM > Install-Package NeoModules.NEP6
```
The NEP6 project includes all the other modules as dependencies.

But you can also install separate modules if you only need one:

```
PM > Install-Package NeoModules.RPC
```

## RPC client

NeoModules first package is the RPC client. This very helpful for projects that need to request info to official nodes, and do not want to waste time on creating a client. It's support all available rpc calls, with DTO's.

Develop with decoupling in mind to make maintenance and new RPC methods implemented more quickly:

* Client base and RPC client implementation - NeoModules.JsonRpc.Client project
* DTO'S, Services, Helpers - NeoModules.RPC (main project)
* Tests - NeoModules.RPC.Tests
* Demo - Simple demonstration project

Setup the rpc client node

```C#
var rpcClient = new RpcClient(new Uri("http://seed5.neo.org:10332"));
var NeoApiService = new NeoApiService(rpcClient);
```

With **NeoApiService** you have all the methods available, organized by:
Accounts,
Assets,
Block,
Contract,
Node,
Transaction

Then you just need to choose the wanted service, call ```SendRequestAsync()``` and pass the necessary parameters if needed.
e.g.

```C#
var accountsService = NeoApiService.Accounts;
var state = accountsService.GetAccountState.SendRequestAsync("ADDRESS HERE");
```

If you don't need all the services, you can simply create an instance of the desired service.

```C#
var blockService = new NeoApiBlockService(new RpcClient(new Uri("http://seed5.neo.org:10332")));
var bestBlockHash  = await blockService.GetBestBlockHash.SendRequestAsync();
```

All rpc calls return a DTO or a simple type like string or int.

### NEP 5 service
You can also create a service to query NEP5 tokens. The bool parameter refers to human readable. So you can get the default rpc response in hexadecimal format, or use the bool = true, to return in a human readable format.
GetTotalSupply and GetBalance need the token decimals to return a correct result.

```C#
var name = await nep5Service.GetName("ed07cffad18f1308db51920d99a2af60ac66a7b3", true);
var decimals = await nep5Service.GetDecimals("ed07cffad18f1308db51920d99a2af60ac66a7b3");
var totalsupply = await nep5Service.GetTotalSupply("ed07cffad18f1308db51920d99a2af60ac66a7b3", 8);
var symbol = await nep5Service.GetSymbol("ed07cffad18f1308db51920d99a2af60ac66a7b3", true);
var balance = await nep5Service.GetBalance("ed07cffad18f1308db51920d99a2af60ac66a7b3",               "0x3640b023405b4b9c818e8387bd01f67bba04dad2", 8);

Debug.WriteLine($"Token info: \nName: {name} \nSymbol: {symbol} \nDecimals: {decimals} \nTotalSupply: {totalsupply} \nBalance: {balance}");

Example:

Token info: 
Name: Trinity Network Credit 
Symbol: TNC 
Decimals: 8 
TotalSupply: 1000000000 
Balance: 1457.82
```

## Rest services

NeoModules features the most popular REST APIs from Neo ecosystem. All the API's return DTO's or simple variables.

### Neoscan service

You can use the official urls
```C# 
var restService = new NeoScanRestService(NeoScanNet.MainNet);
```
or use your own local instance of Neoscan

```C# 
var neoscanService = new NeoScanRestService("https://url.here/api/main_net[or test_net]/v1/");
```

All the endpoints are covered. For the official documentation, refer to https://neoscan.io/docs/index.html#api-v1.
```C#  
var getBalance = await neoscanService.GetBalanceAsync(testAddress);
var getClaimed = await neoscanService.GetClaimedAsync(testAddress);
var getClaimable = await neoscanService.GetClaimableAsync(testAddress);
var getUnclaimed = await neoscanService.GetUnclaimedAsync(testAddress);
var nodes = await neoscanService.GetAllNodesAsync();
var transaction = await neoscanService.GetTransactionAsync(              "610e2a4c7cdc4f311be65cee48e076871b685e13cc750397f4ee5f800da3309a");
var height = await neoscanService.GetHeight();
var abstractAddress = await neoscanService.GetAddressAbstracts("AGbj6WKPUWHze12zRyEL5sx8nGPVN6NXUn", 0);
var addressToAddressAbstract = await neoscanService.GetAddressToAddressAbstract(
                "AJ5UVvBoz3Nc371Zq11UV6C2maMtRBvTJK",
                "AZCcft1uYtmZXxzHPr5tY7L6M85zG7Dsrv", 0);
var block = await neoscanService.GetBlock("54ffd56d6a052567c5d9abae43cc0504ccb8c1efe817c2843d154590f0b572f7");
var lastTransactionsByAddress = await neoscanService.GetLastTransactionsByAddress("AGbj6WKPUWHze12zRyEL5sx8nGPVN6NXUn", 0);
```

### Nodes list

Besides the "get_all_nodes" call on Neoscan API, there also an option to use http://monitor.cityofzion.io/ to get all the nodes with some extra info.

```C# 
var service = new NeoNodesListService();
var result = await service.GetNodesList(MonitorNet.TestNet);
var nodes = JsonConvert.DeserializeObject<NodeList>(result);
```

### Notifications service

This is a service that persists all the events emitted from smart contracts, and provide a REST interface for applications that wish to query those event.

You can use an int representing the node number you want to connect (1-5), or insert the explicit url for the server (your own notification server or others)
```C# 
var notificationService = new NotificationService("https://n1.cityofzion.io/v1");
``` 
All the api points are covered. Examples:

```C# 
var addressNotifications = await notificationService.GetAddressNotifications("AGfGWQeM6md6RtEsTUivQNXhp8p4ytkDMR", 1, null, 13, 2691913, 500);
var blockNotifications = await notificationService.GetBlockNotifications(10);
var contractNotifications = await notificationService.GetContractNotifications("0x67a5086bac196b67d5fd20745b0dc9db4d2930ed");
var tokenList = await notificationService.GetTokens();
var transaction = await notificationService.GetTransactionNotifications("1db9ad15febbf7b9cfa11603ecbe52aad5be6137a9e1d29e83465fa664c2a6ed");
``` 

### HappyNodes service
Happynodes is a blockchain network monitor and visualisation tool designed for the NEO Smart Economy Blockchain. It exposes a REST API that it's fully covered in NeoModules.

Create the service. The default contructor routes to https://api.happynodes.f27.ventures/redis/, but you can use your own if want too:

```C# 
var happyNodesService = new HappyNodesService();
``` 
Again, all API calls are covered:
 
```C# 
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
```

### Switcheo service

Switcheo is the most popular dex (decentralized exchange) on NEO right now. NeoModules also supports the full public API. You can use this service to interact with the dex (place orders, sign message, etc.). Because this service is a lot more complicated that the others mention above, there is a separated project called "NeoModules.SwitcheoDemo" where you have all the calls and features demonstrated. Of course, you can check the official documentation here: https://docs.switcheo.network/.


## NEP6 Compatible Wallet
The wallet creation is of WalletManager.cs responsability. You can use this online or offline, it only depends on the initialization.
The online wallet needs a rest and a rpc client:

```C# 
public WalletManager(INeoRestService restService, IClient client, Wallet wallet = null)
```

### Create an empty wallet and import and account 
You can use wif in string or byte format
```C# 
var walletManager = new WalletManager(new NeoScanRestService(NeoScanNet.MainNet), RpcClient);
var importedAccount = walletManager.ImportAccount("** INSERT WIF HERE **", "Custom account label");
```
Or use NEP6 (this one uses async/await because it can be a heavy operation, especially on mobile hardware)
```C# 
var importedAccount = await walletManager.ImportAccount("** INSERT NEP6 PASSPHRASE HERE **", "** PASSWORD **", "Custom account label");
```

### Transactions
For now, you need to use this check before using the TransactionManager, responsable for the making and signing the transactions. This is needed because NeoModules will use different signing strategies, but for now, only the AccountSignerTransactionManager is available.

### Sending native assets
```C# 
if (importedAccount.TransactionManager is AccountSignerTransactionManager accountSignerTransactionManager)
{
    var sendGasTx = await accountSignerTransactionManager.SendAsset("** INSERT TO ADDRESS HERE **", "GAS", 323.032m);
    var sendNeoT = await accountSignerTransactionManager.SendAsset("** INSERT TO ADDRESS HERE **", "NEO", 13m) 
}
```
### Making a contract call
```C# 
var scriptHash = "** INSERT CONTRACT SCRIPTHASH **".ToScriptHash().ToArray();
var operation = "balanceOf";
var arguments = new object[] { "arg1" };        

var contractCallTx = await accountSignerTransactionManager.CallContract(scriptHash, operation, arguments);
```

### Estimate contract call gas consumption
```C#
var estimateContractGasCall = await accountSignerTransactionManager.EstimateGasContractCall(scriptHash, operation, arguments);
```

### Making a call contract with attached assets
```C#
var assetToAttach = "GAS";
                var output = new List<TransactionOutput>()
                {
                    new TransactionOutput()
                    {
                        AddressHash = "** INSERT TO ADDRESS HERE**".ToScriptHash().ToArray(),
                        Amount = 2,
                    }
                };
var contractCallWithAttachedTx =
                    await accountSignerTransactionManager.CallContract(scriptHash, operation, arguments, assetToAttach,
                        output);
```

### Claiming Gas
```C#
var callGasTx = await accountSignerTransactionManager.ClaimGas();
```

### Transfer NEP5 tokens
```C#
var transferNepTx = await accountSignerTransactionManager.TransferNep5("** INSERT TO ADDRESS HERE**", 32.3m, scriptHash);
```


## Authors

* **Bruno Freitas** - [BrunoFreitasgit](https://github.com/BrunoFreitasgit)
* [Neo](https://github.com/neo-project/)
* [Neo-lux](https://github.com/CityOfZion/neo-lux)
* Multiple base code is taken/inspired from [Nethereum](https://github.com/Nethereum/Nethereum) project

## License

This project is licensed under the MIT License - see the [LICENSE.md](https://github.com/BrunoFreitasgit/Neo-RPC-SharpClient/blob/master/LICENSE) file for details
