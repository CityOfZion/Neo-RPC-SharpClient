using System;
using System.Threading.Tasks;
using NeoModules.JsonRpc.Client;
using NeoModules.RPC.DTOs;
using NeoModules.RPC.Services.Account;
using Xunit;

namespace NeoModules.RPC.Tests.Testers
{
    public class NeoValidateAddressTester : RpcRequestTester<ValidateAddress>
    {
        private string InvalidAddress { get; } = "thisIsAnInvalidAddress";

        [Fact]
        public async void ShouldReturnValid()
        {
            var validAddress = await ExecuteAsync();
            Assert.True(validAddress != null && validAddress.IsValid);
        }

        [Fact]
        public async void ShouldReturnInvalid()
        {
            var validateAddress = new NeoValidateAddress(Client);
            var invalidAddress = await validateAddress.SendRequestAsync(InvalidAddress);
            Assert.False(invalidAddress != null && invalidAddress.IsValid);
        }

        public override async Task<ValidateAddress> ExecuteAsync(IClient client)
        {
            var validateAddress = new NeoValidateAddress(client);
            return await validateAddress.SendRequestAsync(Settings.GetDefaultAccount());
        }

        public override Type GetRequestType()
        {
            return typeof(ValidateAddress);
        }
    }
}
