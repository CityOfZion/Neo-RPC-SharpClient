using System.Collections.Generic;
using System.Threading.Tasks;
using NeoModules.Core;
using NeoModules.JsonRpc.Client;
using NeoModules.NEP6.Interfaces;
using NeoModules.NEP6.Models;
using NeoModules.NEP6.Transactions;
using Transaction = NeoModules.RPC.DTOs.Transaction;
using WalletTransaction = NeoModules.NEP6.Transactions.Transaction;

namespace NeoModules.NEP6.TransactionManagers
{
    public interface ITransactionManager
    {
        IClient Client { get; set; }
        IAccount Account { get; set; }

        string SignMessage(string messageToSign);

        byte[] SignTransaction(WalletTransaction txInput, bool signed = true);

        Task<WalletTransaction> AssetlessContractCall(byte[] scriptHash, byte[] script);

        Task<ClaimTransaction> ClaimGas(UInt160 changeAddress = null);

        Task<WalletTransaction> CallContract(string contractScriptHash, string operation, object[] args,
            IEnumerable<TransferOutput> outputs = null, decimal fee = 0, List<TransactionAttribute> attributes = null);

        Task<WalletTransaction> TransferNep5(List<TransactionAttribute> attributes,
            IEnumerable<TransferOutput> outputs, UInt160 changeAddress = null, decimal fee = 0);

        Task<ContractTransaction> SendNativeAsset(List<TransactionAttribute> attributes,
            IEnumerable<TransferOutput> outputs,
            UInt160 changeAddress = null,
            decimal fee = 0);

        Task<InvocationTransaction> DeployContract(byte[] contractScript, byte[] parameterList,
            ContractParameterType returnType, ContractPropertyState properties,
            string name, string version, string author, string email, string description);

        Task<decimal> EstimateGasContractInvocation(byte[] scriptHash, string operation, object[] args);

        Task<bool> SendTransactionAsync(string serializedAndSignedTx);

        Task<Transaction> WaitForTxConfirmation(string tx, int requestTimeout, int maxAttempts);

        Task<Transaction> GetTransaction(string tx);

    }
}