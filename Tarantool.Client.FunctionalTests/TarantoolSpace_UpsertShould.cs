﻿using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MsgPack.Serialization;
using Tarantool.Client.Models;
using Tarantool.Client.Models.ClientMessages;
using Xunit;

namespace Tarantool.Client
{
    public class TarantoolSpace_UpsertShould
    {
        public class MyTestEntity
        {
            [MessagePackMember(0)]
            public uint MyTestEntityId { get; set; }

            [MessagePackMember(1)]
            public string SomeStringField { get; set; }

            [MessagePackMember(2)]
            public int SomeIntField { get; set; }
        }

        [Fact]
        public async Task UpsertAsInsert()
        {
            var tarantoolClient = TarantoolClient.Create("mytestuser:mytestpass@tarantool-host:3301");
            var testSpace = tarantoolClient.GetSpace<MyTestEntity>("test");

            try
            {
                await testSpace.UpsertAsync(
                    new MyTestEntity
                    {
                        MyTestEntityId = 555,
                        SomeStringField = "Some name",
                        SomeIntField = 1550
                    },
                    new[]
                    {
                        new UpdateOperation<int>
                        {
                            Operation = UpdateOperationCode.Assign,
                            FieldNo = 2,
                            Argument = 1555
                        }
                    });

                var result = await testSpace.SelectAsync(new List<object> { 555u });
                Assert.Equal(1, result.Count);
                Assert.Equal(555u, result[0].MyTestEntityId);
                Assert.Equal("Some name", result[0].SomeStringField);
                Assert.Equal(1550, result[0].SomeIntField);
            }
            finally
            {
                await testSpace.DeleteAsync(new List<object> { 555u });
            }
        }

        [Fact]
        public async Task UpsertAsUpdate()
        {
            var tarantoolClient = TarantoolClient.Create("mytestuser:mytestpass@tarantool-host:3301");
            var testSpace = tarantoolClient.GetSpace<MyTestEntity>("test");

            try
            {
                await testSpace.InsertAsync(new MyTestEntity
                {
                    MyTestEntityId = 544,
                    SomeStringField = "Some name",
                    SomeIntField = 1400
                });

                await testSpace.UpsertAsync(
                    new MyTestEntity
                    {
                        MyTestEntityId = 544,
                        SomeStringField = "Some name",
                        SomeIntField = 1440
                    },
                    new[]
                    {
                        new UpdateOperation<int>
                        {
                            Operation = UpdateOperationCode.Assign,
                            FieldNo = 2,
                            Argument = 1444
                        }
                    });

                var result = await testSpace.SelectAsync(new List<object> { 544u });
                Assert.Equal(1, result.Count);
                Assert.Equal(544u, result[0].MyTestEntityId);
                Assert.Equal("Some name", result[0].SomeStringField);
                Assert.Equal(1444, result[0].SomeIntField);
            }
            finally
            {
                await testSpace.DeleteAsync(new List<object> { 544u });
            }
        }
    }
}