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
        public static IEnumerable<T> GenerateTestCommands<T>()
            where T : StorageCommand, new()
        {
            var random = new Random(Guid.NewGuid().GetHashCode());
            var buffer = new byte[random.Next() % (5 << 10) + (2 << 10)];
            random.NextBytes(buffer);
            yield return new T
            {
                Key = "1234567890-=~!@#$%^&*()_+",
                Data = new ArraySegment<byte>(buffer),
            };
            yield return new T
            {
                Key = "1234567890-=~!@#$%^&*()_+0aweawefa;jk",
                Data = new ArraySegment<byte>(buffer, 21, 1 << 10 + 1),
                Options = new MemcachedStorageOptions
                {
                    Flags = 123,
                }
            };
            yield return new T
            {
                Key = "1234567890-=~!@#$%^&*()_+0aweawefa;jk",
                Data = new ArraySegment<byte>(buffer, 21, 1 << 10 + 1),
                Options = new MemcachedStorageOptions
                {
                    Flags = 234225,
                    ExpirationTime = TimeSpan.FromHours(2) + TimeSpan.FromMilliseconds(53),
                }
            };
        }

        public static async Task TestSendBehavior(StorageCommand command)
        {
            var mockSocket = new MockSocket();
            await command.SendRequest(mockSocket);

            using (var stream = mockSocket.GetSentData())
            {
                // validate the command line
                var commandLine = ReadLine(stream);
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

        private static string ReadLine(Stream stream, Encoding encoding = null)
        {
            encoding = encoding ?? Encoding.UTF8;
            var result = new StringBuilder();
            var decoder = encoding.GetDecoder();
            while (true)
            {
                var val = ReadMinimum(stream, decoder);
                if (val == null) throw new EndOfStreamException();
                if (val == "\n") return result.ToString();
                if (val != "\r")
                {
                    result.Append(val);
                }
            }
        }

        private static string ReadMinimum(Stream stream, Decoder decoder)
        {
            var buffer = new byte[16];
            for (int i = 0; i < buffer.Length; ++i)
            {
                var read = stream.Read(buffer, i, 1);
                if (read == 0)
                {
                    if (i != 0) throw new Exception("invalid encoded text?");
                    return null;
                }
                var charCount = decoder.GetCharCount(buffer, 0, i + 1);
                if (charCount != 0)
                {
                    var chars = new char[charCount];
                    decoder.GetChars(buffer, 0, i + 1, chars, 0);
                    return new string(chars);
                }
            }
            throw new Exception("impossible?");
        }
    }
}
