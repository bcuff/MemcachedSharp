using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MemcachedSharp.Commands;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace MemcachedSharp.Test.Commands
{
    internal static class RetrievalCommandValidator
    {
        public static IEnumerable<T> GenerateTestCommands<T>()
            where T : RetrievalCommand, new()
        {
            yield return new T { Key = "{}[]`12345678900-=~!@#$%^&*().,/*-\\?~:\";'" };
            yield return new T { Key = "ひらがな" };
            yield return new T { Key = new string(Enumerable.Repeat('a', 250).ToArray()) };
        }

        public static async Task TestSendBehavior(string expectedVerb, RetrievalCommand command)
        {
            Assert.AreEqual(expectedVerb, command.Verb);
            var socket = new MockSocket();
            await command.SendRequest(socket);
            using (var stream = socket.GetSentData())
            {
                var line = stream.ReadLine();
                Assert.AreEqual(expectedVerb + " " + command.Key, line);
                Assert.AreEqual(stream.Position, stream.Length);
            }
        }

        public static async Task TestSendBehavior<T>(string expectedVerb)
            where T : RetrievalCommand, new()
        {
            foreach (var command in GenerateTestCommands<T>())
            {
                await TestSendBehavior(expectedVerb, command);
            }
        }
    }
}
