using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MemcachedSharp.Commands;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace MemcachedSharp.Test.Commands
{
    internal static class StorageCommandValidator
    {
        public static IEnumerable<TCommand> GenerateTestCommands<TCommand, TResult>()
            where TCommand : StorageCommand<TResult>, new()
        {
            var random = new Random(Guid.NewGuid().GetHashCode());
            var buffer = new byte[random.Next() % (5 << 10) + (2 << 10)];
            random.NextBytes(buffer);
            yield return new TCommand
            {
                Key = "1234567890-=~!@#$%^&*()_+ひらがな",
                Data = new ArraySegment<byte>(buffer),
            };
            yield return new TCommand
            {
                Key = "1234567890-=~!@#$%^&*()_+0aweawefa;jk",
                Data = new ArraySegment<byte>(buffer, 21, 1 << 10 + 1),
                Options = new MemcachedStorageOptions
                {
                    Flags = 123,
                }
            };
            yield return new TCommand
            {
                Key = "1234567890-=~!@#$%^&*()_+0aweawefa;jk{}[]`12345678900-=~!@#$%^&*().,/*-\\?~:\";'",
                Data = new ArraySegment<byte>(buffer, 21, 1 << 10 + 1),
                Options = new MemcachedStorageOptions
                {
                    Flags = 234225,
                    ExpirationTime = TimeSpan.FromHours(2) + TimeSpan.FromMilliseconds(53),
                }
            };
        }

        public static async Task TestSendBehavior<TCommand, TResult>(string expectedVerb)
            where TCommand : StorageCommand<TResult>, new()
        {
            foreach (var command in GenerateTestCommands<TCommand, TResult>())
            {
                await TestSendBehavior(expectedVerb, command);
            }
        }

        public static async Task TestSendBehavior<T>(string expectedVerb, StorageCommand<T> command)
        {
            var mockSocket = new MockSocket();
            await command.SendRequest(mockSocket);

            using (var stream = mockSocket.GetSentData())
            {
                // validate the command line
                var commandLine = stream.ReadLine();
                Assert.IsNotNull(commandLine);
                var parts = commandLine.Split(' ');
                Assert.AreEqual(5, parts.Length);
                Assert.AreEqual(command.Verb, parts[0]);
                Assert.AreEqual(command.Key, parts[1]);
                if (command.Options == null)
                {
                    Assert.AreEqual("0", parts[2]);
                    Assert.AreEqual("0", parts[3]);
                }
                else
                {
                    Assert.AreEqual(command.Options.Flags.ToString(), parts[2]);
                    var expectedExpires = command.Options.ExpirationTime.HasValue
                        ? ((uint)(command.Options.ExpirationTime.Value.TotalSeconds)).ToString()
                        : "0";
                    Assert.AreEqual(expectedExpires, parts[3]);
                }
                Assert.AreEqual(command.Data.Count.ToString(), parts[4]);

                // validate the data block
                var block = new byte[command.Data.Count];
                var read = stream.Read(block, 0, block.Length);
                Assert.AreEqual(block.Length, read);
                var expected = new byte[command.Data.Count];
                Buffer.BlockCopy(command.Data.Array, command.Data.Offset, expected, 0, command.Data.Count);
                CollectionAssert.AreEqual(expected, block);

                // read the last endline
                Assert.AreEqual('\r', stream.ReadByte());
                Assert.AreEqual('\n', stream.ReadByte());
                Assert.AreEqual(-1, stream.ReadByte());
            }
        }

        public static async Task AssertReadResponseFailure<TCommand, TResult>(TCommand command, StorageCommandResult result)
            where TCommand : StorageCommand<TResult>
        {
            var response = Encoding.UTF8.GetBytes(result.ToVerb() + "\r\n");
            using (var ms = new MemoryStream(response))
            {
                var reader = new MemcachedResponseReader(ms, Encoding.UTF8, TimeSpan.FromSeconds(2));
                try
                {
                    await command.ReadResponse(reader);
                    Assert.Fail("Response for " + result + " was expected to fail but succeeded.");
                }
                catch (MemcachedException mex)
                {
                    StringAssert.StartsWith(mex.Message, "Unexpected response");
                    StringAssert.Contains(mex.Message, result.ToVerb());
                }
            }
        }

        public static async Task AssertReadResponse<TCommand, TResult>(TCommand command, StorageCommandResult result, TResult expectedResult)
            where TCommand : StorageCommand<TResult>
        {
            var response = Encoding.UTF8.GetBytes(result.ToVerb() + "\r\n");
            using (var ms = new MemoryStream(response))
            {
                var reader = new MemcachedResponseReader(ms, Encoding.UTF8, TimeSpan.FromSeconds(2));
                var actualResult = await command.ReadResponse(reader);
                Assert.AreEqual(expectedResult, actualResult);
            }
        }
    }
}
