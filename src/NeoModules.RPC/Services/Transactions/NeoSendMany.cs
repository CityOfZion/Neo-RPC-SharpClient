using NeoModules.JsonRpc.Client;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace NeoModules.RPC.Services.Transactions
{
	/// <summary>
	///     sendmany  
	///     Transfers to the specified address.
	/// 
	///     Parameters
	///			Array - Asset_id: Asset ID(asset identifier), which is the transaction ID of the RegistTransaction when the asset is registered.
	///					For NEO: c56f33fc6ecfcd0c225c4ab356fee59390af8560be0e930faebe74a6daff7c9b
	///					For GAS: 602c79718b16e442de58778e148d0b1084e3b2dffd5de6b7b16cee7969282de7
	///					The remaining asset IDs can be passed through the CLI commandline, the list Asset command query can also be queried in the block chain browser.
	///				Address: Payment address
	///				Value: Amount transferred
	///			Fee: Fee, default value is 0 (optional parameter)
	///			Change_address: Change address, optional parameter, default is the first standard address in the wallet.
	/// 
	///     Returns
	///     Returning of the transaction details above, indicates that the transaction was sent successfully. If not, the transaction has failed to send.
	///     If the signature is incomplete, it will return the transaction to be signed.
	///     If the balance is insufficient, it will return an error message.
	/// 
	///     Example
	///     Request
	///     curl -X POST --data '{"jsonrpc":"2.0","method":"sendmany","params": [[{"asset": "025d82f7b00a9ff1cfe709abe3c4741a105d067178e645bc3ebad9bc79af47d4","value": 1,"address": "AbRTHXb9zqdqn5sVh4EYpQHGZ536FgwCx2"},{"asset": "025d82f7b00a9ff1cfe709abe3c4741a105d067178e645bc3ebad9bc79af47d4","value": 1,"address": "AbRTHXb9zqdqn5sVh4EYpQHGZ536FgwCx2"}]],"id":1}'
	/// 
	///     Result
	///     {
	///     "jsonrpc": "2.0",
	///     "id": 1,
	///     "result": {
	///  "Txid": "27b9a82ed519eec17c5520927b3f472e4df28b835c24dba25645e1650ed8d2ac",
	///  "Size": 322,
	///  "Type": "ContractTransaction",
	///  "Version": 0,
	///  "Attributes":[],
	///  "Vin": [
	///    { 
	///      "Txid": "8674c38082e59455cf35cee94a5a1f39f73b617b3093859aa199c756f7900f1f",
	///      "Vout": 0
	///    }
	///  ],
	///  "vout": [
	///    {
	///      "N": 0,
	///      "Asset": "025d82f7b00a9ff1cfe709abe3c4741a105d067178e645bc3ebad9bc79af47d4",
	///      "Value": "1",
	///      "Address": "AbRTHXb9zqdqn5sVh4EYpQHGZ536FgwCx2"
	///    },
	///    {
	///      "n": 1,
	///      "Asset": "025d82f7b00a9ff1cfe709abe3c4741a105d067178e645bc3ebad9bc79af47d4",
	///      "Value": "1",
	///      "Address": "AbRTHXb9zqdqn5sVh4EYpQHGZ536FgwCx2"
	///     }
	///  ],
	///  "Sys_fee": "0",
	///  "Net_fee": "0",
	///  "Scripts": [
	///     {
	///      "Invocation": "40844144eb6819cb094afee2db5e5da078cfc7bbe29dbc60e47b4c3b4bdf77a5fd97865ae9b5a8d8bb3fa20f1441a58a05f848b2ea49c6c0dbbfc5ed241b226665",
	///      "Verification": "210208c5203d32f960c54c225f140c1020408b114c15d29082fc959dac6874828fccac"
	///      }
	///   ]
	///}
	/// }
	/// </summary>
	public class NeoSendMany : RpcRequestResponseHandler<DTOs.Transaction>
	{
		public NeoSendMany(IClient client) : base(client, ApiMethods.sendmany.ToString())
		{
		}

		public Task<DTOs.Transaction> SendRequestAsync(List<DTOs.SendManyParameter> parameters, decimal fee = 0, string changeAddress = null, object id = null)
		{
			if (parameters.Count == 0) throw new ArgumentOutOfRangeException(nameof(parameters));
			if (fee < 0) throw new ArgumentOutOfRangeException(nameof(fee));

			if (changeAddress == null) return base.SendRequestAsync(id, parameters, fee);
			return base.SendRequestAsync(id, parameters, fee, changeAddress);
		}

		public RpcRequest BuildRequest(List<DTOs.SendManyParameter> parameters, decimal fee = 0, string changeAddress = null, object id = null)
		{
			if (parameters.Count == 0) throw new ArgumentOutOfRangeException(nameof(parameters));
			if (fee < 0) throw new ArgumentOutOfRangeException(nameof(fee));

			if (changeAddress == null) return base.BuildRequest(id, parameters, fee);
			return base.BuildRequest(id, parameters, fee, changeAddress);
		}
	}
}
