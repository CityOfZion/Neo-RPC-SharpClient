using NeoModules.JsonRpc.Client;
using NeoModules.RPC.Infrastructure;

namespace NeoModules.RPC.Services.Account
{
    /// <Summary>
    ///     getwalletheight    
    ///     Obtains the current wallet index height.
    /// 
    ///     Parameters
    ///     none
    /// 
    ///     Returns
    ///     Current wallet index height.
    /// 
    ///     Example
    ///     Request
    ///     curl -X POST --data '{"jsonrpc":"2.0","method":"getwalletheight","params":[],"id":1}'
    /// 
    ///     Result
    ///     {
    ///     "jsonrpc": "2.0",
    ///     "id": 1,
    ///     "result": 2713183
    /// }
    /// </Summary>
    public class NeoGetWalletHeight: GenericRpcRequestResponseHandlerNoParam<int>
    {
        public NeoGetWalletHeight(IClient client) : base(client, ApiMethods.getwalletheight.ToString())
        {
        }
    }
}
