using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using NeoModules.JsonRpc.Client;
using NeoModules.RPC;
using NeoModules.RPC.DTOs;
using NeoModulesCore.Models;

namespace NeoModulesCore.Controllers
{
    public class RpcClientController : Controller
    {
        private readonly NeoApiService _neoRpcService = new NeoApiService(new RpcClient(new Uri("https://seed0.cityofzion.io:443")));

        public IActionResult Index()
        {
            return View();
        }

        [HttpPost(Name = "ValidateAddress")]
        public async Task<ActionResult<bool>> ValidateAddress(string input)
        {
            if (!string.IsNullOrEmpty(input))
            {
                var result = await _neoRpcService.Accounts.ValidateAddress.SendRequestAsync(input);
                return result.IsValid;
            }

            return false;
        }

        [HttpPost(Name = "ValidateAddress")]
        public async Task<ActionResult<string>> GetBestBlockHash()
        {
            var result = await _neoRpcService.Blocks.GetBestBlockHash.SendRequestAsync();
            return result;
        }

        public async Task<ActionResult<Block>> GetBlock(string input)
        {
            if (!string.IsNullOrEmpty(input))
            {
                var result = await _neoRpcService.Blocks.GetBlock.SendRequestAsync(input);
                return result;
            }

            return null;
        }
    }
}