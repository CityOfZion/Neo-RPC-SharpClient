using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using NeoModules.Core;
using NeoModules.Core.KeyPair;
using NeoModules.JsonRpc.Client;
using NeoModules.NEP6.Interfaces;
using NeoModules.NEP6.Models;
using NeoModules.NEP6.Transactions;
using NeoModules.RPC.DTOs;
using NeoModules.RPC.Services.Contract;
using NeoModules.RPC.Services.Transactions;
using DTOTransaction = NeoModules.RPC.DTOs.Transaction;
using Transaction = NeoModules.NEP6.Transactions.Transaction;
using Utils = NeoModules.NEP6.Helpers.Utils;

namespace NeoModules.NEP6.TransactionManagers
{
    public abstract class TransactionManagerBase : ITransactionManager
    {
        public IClient Client { get; set; }
        public IAccount Account { get; set; }

        /// <summary>
        ///     Makes a 'invokescript' RPC call to the connected node.
        ///     Return the gas cost if the contract tx is "simulated" correctly
        /// </summary>
        /// <param name="scriptHash"></param>
        /// <param name="operation"></param>
        /// <param name="args"></param>
        /// <returns></returns>
        public virtual async Task<decimal> EstimateGasContractInvocation(byte[] scriptHash, string operation,
            object[] args)
        {
            var bytes = Utils.GenerateScript(scriptHash.ToScriptHash(), operation, args);
            return await EstimateGasAsync(bytes.ToHexString());
        }

        public virtual async Task<bool> SendTransactionAsync(string signedTx)
        {
            if (Client == null) throw new NullReferenceException("Client not configured");
            if (signedTx == null) throw new ArgumentNullException(nameof(signedTx));
            var neoSendRawTransaction = new NeoSendRawTransaction(Client);
            return await neoSendRawTransaction.SendRequestAsync(signedTx);
        }

        /// <summary>
        ///     Makes a 'getrawtransaction ' RPC call to the connected node.
        ///     Only returns if the Transaction already has a block hash (indicates that is part of a block, therefore is
        ///     confirmed)
        /// </summary>
        /// <param name="tx"></param>
        /// <param name="requestTimeout"></param>
        /// <param name="maxAttempts"></param>
        /// <returns></returns>
        public virtual async Task<DTOTransaction> WaitForTxConfirmation(string tx, int requestTimeout, int maxAttempts)
        {
            while (maxAttempts > 0)
            {
                var confirmedTx = await GetTransaction(tx);
                if (confirmedTx != null && !string.IsNullOrEmpty(confirmedTx.Blockhash)) return confirmedTx;
                await Task.Delay(requestTimeout);
                maxAttempts--;
            }

            return null;
        }

        public virtual async Task<DTOTransaction> GetTransaction(string tx)
        {
            if (Client == null) throw new NullReferenceException("Client not configured");
            if (string.IsNullOrEmpty(tx)) throw new ArgumentNullException(nameof(tx));
            var neoGetRawTransaction = new NeoGetRawTransaction(Client);
            return await neoGetRawTransaction.SendRequestAsync(tx);
        }

        public virtual async Task<decimal> EstimateGasAsync(string serializedScriptHash)
        {
            if (Client == null) throw new NullReferenceException("Client not configured");
            if (serializedScriptHash == null) throw new ArgumentNullException(nameof(serializedScriptHash));
            var neoEstimateGas = new NeoInvokeScript(Client);
            var invokeResult = await neoEstimateGas.SendRequestAsync(serializedScriptHash);
            return invokeResult.GasConsumed;
        }

        public virtual async Task<decimal> EstimateGasAsync(string scriptHash, string operation,
            List<InvokeParameter> parameterList)
        {
            if (Client == null) throw new NullReferenceException("Client not configured");
            if (scriptHash == null) throw new ArgumentNullException(nameof(scriptHash));
            var neoEstimateGas = new NeoInvokeFunction(Client);
            var invokeResult = await neoEstimateGas.SendRequestAsync(scriptHash, operation, parameterList);
            return invokeResult.GasConsumed;
        }

        #region Abstract Methods

        public abstract string SignMessage(string messageToSign);

        public abstract byte[] SignTransaction(Transaction txInput, bool signed = true);

        public abstract Task<Transaction> AssetlessContractCall(byte[] scriptHash, byte[] script);

        public abstract Task<ClaimTransaction> ClaimGas(UInt160 changeAddress = null);

        public abstract Task<Transaction> CallContract(string contractScriptHash, string operation, object[] args,
            IEnumerable<TransferOutput> outputs = null, decimal fee = 0, List<TransactionAttribute> attributes = null);

        public abstract Task<Transaction> TransferNep5(List<TransactionAttribute> attributes,
            IEnumerable<TransferOutput> outputs, UInt160 changeAddress = null, decimal fee = 0);

        public abstract Task<ContractTransaction> SendNativeAsset(List<TransactionAttribute> attributes,
            IEnumerable<TransferOutput> outputs,
            UInt160 changeAddress = null,
            decimal fee = 0);

        public abstract Task<InvocationTransaction> DeployContract(byte[] contractScript, byte[] parameterList,
            ContractParameterType returnType, ContractPropertyState properties,
            string name, string version, string author, string email, string description);

        #endregion
    }
}