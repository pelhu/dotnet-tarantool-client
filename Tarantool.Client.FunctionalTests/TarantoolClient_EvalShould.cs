﻿using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Tarantool.Client.Models.ClientMessages;
using Xunit;

namespace Tarantool.Client
{
    public class TarantoolClient_EvalShould
    {
        [Fact]
        public async Task EvaluateScalars()
        {
            var tarantoolClient =
                new TarantoolClient("mytestuser:mytestpass@tarantool-host:3301");

            var result = (await tarantoolClient.EvalAsync(new EvalRequest
            {
                Expression = "return 12345, 23456, 34567"
            })).AsList();

            Assert.Equal(new[] { 12345, 23456, 34567 }, result.Select(x => x.AsInt32()));
        }

        [Fact]
        public async Task EvaluateScalarsFromArguments()
        {
            var tarantoolClient =
                new TarantoolClient("mytestuser:mytestpass@tarantool-host:3301");

            var result = (await tarantoolClient.EvalAsync(new EvalRequest
            {
                Expression = "return ...",
                Args = new List<object> { 912345, 923456, 934567 }
            })).AsList();

            Assert.Equal(new[] { 912345, 923456, 934567 }, result.Select(x => x.AsInt32()));
        }
    }
}