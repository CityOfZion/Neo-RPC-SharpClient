using NeoModules.JsonRpc.Client;
using System;
using System.Threading.Tasks;

namespace NeoModules.RPC.Services.Transactions
{
	/// <summary>
	///     sendfrom   
	///     Transfer from the specified address to the designated address.
	///		<note type="important"> You need to open the wallet in the Neo-CLI node before executing this command.</note>
	///		
	///     Parameters
	///			Asset_id: Asset ID(asset identifier), which is the transaction ID of the RegistTransaction when the asset is registered.
	///				For NEO: c56f33fc6ecfcd0c225c4ab356fee59390af8560be0e930faebe74a6daff7c9b
	///				For GAS: 602c79718b16e442de58778e148d0b1084e3b2dffd5de6b7b16cee7969282de7
	///				The remaining asset IDs can be queried through the list asset command in CLI Command or in the Block Chain Browser.
	///      From: transfer address.
	///		 To: Payment address.
	///		 Value: transfer amount.
	///      Fee: Fee, default value is 0 (optional parameter).
	/// 
	///     Returns
	///     Returning of the transaction details above, indicates that the transaction was sent successfully. If not, the transaction has failed to send.
	///     If the signature is incomplete, it will return the transaction to be signed.
	///     If the balance is insufficient, it will return an error message.
	/// 
	///     Example
	///     Request
	///     curl -X POST --data '{"jsonrpc":"2.0","method":"sendfrom","params":["025d82f7b00a9ff1cfe709abe3c4741a105d067178e645bc3ebad9bc79af47d4", "AK4if54jXjSiJBs6jkfZjxAastauJtjjse", 1],"id":1}'
	/// 
	///     Result
	///     {
	///     "jsonrpc": "2.0",
	///     "id": 1,
	///     "result": {
	///  "Txid": "fbd69da6996cc0896691a35cba2d3b2e429205a12307cd2bdea5fbdf78dc9925",
	///  "Size": 262,
	///  "Type": "ContractTransaction",
	///  "Version": 0,
	///  "Attributes":[],
	///  "Vin": [
	///    { 
	///      "Txid": "19fbe968be17f4bd7b7f4ce1d27e39c5d8a857bd3507f76c653d204e1e9f8e63",
	///      "Vout": 0
	///    }
	///  ],
	///  "Vout": [
	///    {
	///      "N": 0,
	///      "Asset": "025d82f7b00a9ff1cfe709abe3c4741a105d067178e645bc3ebad9bc79af47d4",
	///      "Value": "1",
	///      "Address": "AK4if54jXjSiJBs6jkfZjxAastauJtjjse"
	///    },
	///    {
	///      "N": 1,
	///      "Asset": "025d82f7b00a9ff1cfe709abe3c4741a105d067178e645bc3ebad9bc79af47d4",
	///      "Value": "4978980",
	///      "Address": "AK4if54jXjSiJBs6jkfZjxAastauJtjjse"
	///     }
	///  ],
	///  "Sys_fee": "0",
	///  "Net_fee": "0",
	///  "Scripts": [
	///     {
	///      "Invocation": "40f02345c7e90245F085d0c588433ca9e89c6df58f3636b5240288aab5f081b1c67c3cad5946890de9001fcfe8d8b748b647b116891e6f1fb2393cc2f1aba45a81",
	///      "Verification": "21027b30333e0d0e6552ae6d1da9f9409f551e35ee9719305e945dc4dcba998456caac"
	///      }
	///   ]
	///}
	/// }
	/// </summary>
	public class NeoSendFrom : RpcRequestResponseHandler<DTOs.Transaction>
	{
		public NeoSendFrom(IClient client) : base(client, ApiMethods.sendfrom.ToString())
		{
		}

		public Task<DTOs.Transaction> SendRequestAsync(string assetId, string transferAddress, string paymentAddress, decimal amount, decimal fee = 0, object id = null)
		{
			if (string.IsNullOrEmpty(assetId)) throw new ArgumentNullException(nameof(assetId));
			if (string.IsNullOrEmpty(transferAddress)) throw new ArgumentNullException(nameof(transferAddress));
			if (string.IsNullOrEmpty(paymentAddress)) throw new ArgumentNullException(nameof(paymentAddress));
			if (amount <= 0) throw new ArgumentException("Transfer amount must be greater than 0", nameof(amount));

			return base.SendRequestAsync(id, assetId, transferAddress, paymentAddress, amount, fee);
		}

		public RpcRequest BuildRequest(string assetId, string transferAddress, string paymentAddress, decimal amount, decimal fee = 0, object id = null)
		{
			if (string.IsNullOrEmpty(assetId)) throw new ArgumentNullException(nameof(assetId));
			if (string.IsNullOrEmpty(transferAddress)) throw new ArgumentNullException(nameof(transferAddress));
			if (string.IsNullOrEmpty(paymentAddress)) throw new ArgumentNullException(nameof(paymentAddress));
			if (amount <= 0) throw new ArgumentException("Transfer amount must be greater than 0", nameof(amount)); ;

			return base.BuildRequest(id, assetId, transferAddress, paymentAddress, amount, fee);
		}
	}
}
