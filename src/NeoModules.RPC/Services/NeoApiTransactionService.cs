using NeoModules.JsonRpc.Client;
using NeoModules.RPC.Services.Transactions;

namespace NeoModules.RPC.Services
{
    public class NeoApiTransactionService : RpcClientWrapper
    {
        public NeoApiTransactionService(IClient client) : base(client)
        {
            GetRawTransaction = new NeoGetRawTransaction(client);
            SendRawTransaction = new NeoSendRawTransaction(client);
            GetTransactionOutput = new NeoGetTransactionOutput(client);
        }

        public NeoGetRawTransaction GetRawTransaction { get; private set; }
        public NeoSendRawTransaction SendRawTransaction { get; private set; }
        public NeoGetTransactionOutput GetTransactionOutput { get; private set; }
       
    }
}
