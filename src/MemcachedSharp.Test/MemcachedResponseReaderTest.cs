using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace MemcachedSharp.Test
{
    [TestClass]
    public class MemcachedResponseReaderTest
    {
        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void TestConstructorThrowsArgumentNull()
        {
            new MemcachedResponseReader(null, null);
        }

        [TestMethod]
        public async Task TestReadLine()
        {
            using (var stream = new MemoryStream())
            using (var writer = new StreamWriter(stream))
            {
                var line = "foobar some line !@#$%^&*()";
                var parts = line.Split(' ');
                writer.Write(line + "\r\n");
                writer.Write("some other line");
                writer.Write("\r\n");
                writer.Flush();

                stream.Position = 0;

                var reader = new MemcachedResponseReader(stream, writer.Encoding);

                var resultLine = await reader.ReadLine();
                Assert.AreEqual(line, resultLine.Line);
                CollectionAssert.AreEqual(parts, resultLine.Parts);

                Assert.AreEqual("some other line", (await reader.ReadLine()).Line);
            }
        }

        [TestMethod]
        [ExpectedException(typeof(MemcachedException))]
        public async Task TestReadLineValidation()
        {
            using (var stream = new MemoryStream())
            using (var writer = new StreamWriter(stream))
            {
                var line = "ERROR something bad happened";
                writer.Write(line);
                writer.Write("\r\n");
                writer.Flush();

                var reader = new MemcachedResponseReader(stream, writer.Encoding);
                await reader.ReadLine();
            }
        }

        [TestMethod]
        public async Task TestReadItem()
        {
            using (var stream = new MemoryStream())
            using (var writer = new StreamWriter(stream))
            {
                var keys = new[] { "9CADB4CA-64C4-4724-84E8-1B4602D54F0C", "aasodfiawea_)(*&^%$#@!", "foobar" };
                var values = new[] { "test value", "Hello, world!", string.Join(string.Empty, Enumerable.Range(0, 10000).Select(i => (i % 10))) };
                var flags = new uint[] { 12345, 0, 123917070 };
                var casUnique = new long?[] { 0L, long.MaxValue, null };

                for (int i = 0; i < keys.Length; ++i)
                {
                    WriteItem(writer, keys[i], values[i], flags[i], casUnique[i]);
                }

                writer.Write("END\r\n");
                writer.Flush();

                stream.Position = 0;
                var reader = new MemcachedResponseReader(stream, writer.Encoding);
                for (int i = 0; i < keys.Length; ++i)
                {
                    var item = await reader.ReadItem();
                    Assert.AreEqual(keys[i], item.Key);
                    using (var sr = new StreamReader(item.Data, writer.Encoding))
                    {
                        Assert.AreEqual(values[i], sr.ReadToEnd());
                    }
                    Assert.AreEqual(flags[i], item.Flags);
                    Assert.AreEqual(casUnique[i], item.CasUnique);
                }

                Assert.AreEqual(null, await reader.ReadItem());
            }
        }

        private void WriteItem(StreamWriter writer, string key, string value, uint flags, long? casUnique)
        {
            var bytes = writer.Encoding.GetByteCount(value);
            writer.Write("VALUE {0} {1} {2}{3}\r\n", key, flags, bytes, casUnique.HasValue ? " " + casUnique.Value : string.Empty);
            writer.Write(value);
            writer.Write("\r\n");
        }
    }
}
