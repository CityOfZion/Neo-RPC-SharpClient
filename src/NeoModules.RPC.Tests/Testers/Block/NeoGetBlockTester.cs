using System;
using System.Threading.Tasks;
using NeoModules.JsonRpc.Client;
using NeoModules.RPC.DTOs;
using NeoModules.RPC.Services.Block;
using Xunit;

namespace NeoModules.RPC.Tests.Testers
{
    public class NeoGetBlockTester : RpcRequestTester<Block>
    {
        [Fact]
        public async void ShouldReturnBlockWithHash()
        {
            var getBlock = new NeoGetBlock(Client);
            var blockByIndex = await getBlock.SendRequestAsync(Settings.GetBlockHash());
            Assert.NotNull(blockByIndex);
        }

        [Fact]
        public async Task ShouldReturnBlockWithIndex()
        {
            var blockByIndex = await ExecuteAsync();
            Assert.NotNull(blockByIndex);
        }

        public override async Task<Block> ExecuteAsync(IClient client)
        {
            var block = new NeoGetBlock(client);
            return await block.SendRequestAsync(1);
        }

        public override Type GetRequestType()
        {
            return typeof(Block);
        }
    }
}
