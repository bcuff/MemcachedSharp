using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MemcachedSharp.Commands;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace MemcachedSharp.Test.Commands
{
    class ArithmeticalCommandValidator
    {
        public static IEnumerable<T> GenerateTestCommands<T>()
            where T : ArithmeticalCommand, new()
        {
            var random = new Random(Guid.NewGuid().GetHashCode());
            var buffer = new byte[random.Next() % (5 << 10) + (2 << 10)];
            random.NextBytes(buffer);
            yield return new T
            {
                Key = "1234567890-=~!@#$%^&*()_+ひらがな",
                Value = 123,
            };
            yield return new T
            {
                Key = "1234567890-=~!@#$%^&*()_+0aweawefa;jk",
                Value = int.MaxValue,
            };
            yield return new T
            {
                Key = "1234567890-=~!@#$%^&*()_+0aweawefa;jk{}[]`12345678900-=~!@#$%^&*().,/*-\\?~:\";'",
                Value = 0,
            };
        }

        public static async Task TestSendBehavior<T>(string expectedVerb)
            where T : ArithmeticalCommand, new()
        {
            foreach (var command in GenerateTestCommands<T>())
            {
                await TestSendBehavior(expectedVerb, command);
            }
        }

        public static async Task TestSendBehavior(string expectedVerb, ArithmeticalCommand command)
        {
            var mockSocket = new MockSocket();
            await command.SendRequest(mockSocket);

            using (var stream = mockSocket.GetSentData())
            {
                // validate the command line
                var commandLine = stream.ReadLine();
                Assert.IsNotNull(commandLine);
                var parts = commandLine.Split(' ');
                Assert.AreEqual(3, parts.Length);
                Assert.AreEqual(command.Verb, parts[0]);
                Assert.AreEqual(command.Key, parts[1]);
                Assert.AreEqual(command.Value.ToString(), parts[2]);
                Assert.AreEqual(stream.Length, stream.Position);
            }
        }
    
    }
}
