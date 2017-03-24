﻿using System.Threading.Tasks;
using Xunit;

namespace Tarantool.Client
{
    public class TarantoolClient_FindSpaceByNameShould
    {
        [Fact]
        public async Task SelectSpaceId()
        {
            var tarantoolClient =
                new TarantoolClient("mytestuser:mytestpass@tarantool-host:3301");

            var result = await tarantoolClient.FindSpaceByNameAsync("_vspace");

            Assert.NotNull(result);
            Assert.Equal(281, result[0].AsInt32());
            Assert.Equal("_vspace", result[2].AsString());
        }
    }
}