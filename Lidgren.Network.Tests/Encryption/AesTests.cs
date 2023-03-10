using NUnit.Framework;

using System;
using System.Diagnostics;
using System.Text;

namespace Lidgren.Network.Tests.Encryption
{
    [TestFixture]
    public class AesTests : PeerTestFixture
    {
        private const string LoremIpsum = "Lorem ipsum dolor sit amet, consectetur adipiscing elit. Proin viverra augue eget enim convallis ultricies. Vivamus rhoncus blandit nibh ut posuere. Aenean nec neque accumsan, egestas sem eu, molestie sem.";

        [TestCase(true)]
        [TestCase(false)]
        public void TestPaddingBytes(bool forceNewTransform)
        {
            if (!forceNewTransform)
            {
                Assert.Ignore();
            }

            var bytes = Encoding.UTF8.GetBytes(LoremIpsum);
            var length = 0;
            var algorithm = new NetAESEncryption(Peer)
            {
                ForceNewTransform = forceNewTransform
            };

            while (length++ < bytes.Length)
            {
                var outMessage = Peer.CreateMessage();
                outMessage.Write(bytes, 0, length);

                var outBits = outMessage.LengthBits;
                Assert.AreEqual(length << 3, outBits, $"Input data was {length}B ({length << 3}b), message is {outBits}b.");
                Assert.IsTrue(outMessage.Encrypt(algorithm), $"Failed to encrypt [length={length}]");

                var inMessage = outMessage.ToIncomingMessage();

                Assert.IsNotNull(inMessage.Data);
                Assert.IsNotEmpty(inMessage.Data);

                Assert.IsTrue(inMessage.Decrypt(algorithm), "Failed to decrypt");
                Assert.IsNotNull(inMessage.Data);
                Assert.IsNotEmpty(inMessage.Data);
                Assert.AreEqual(outBits, inMessage.LengthBits);

                var inData = inMessage.PeekDataBuffer();
                for (var index = 0; index < length; ++index)
                {
                    Assert.AreEqual(bytes[index], inData[index], $"Arrays differed at index {index} (out of {length}).\nExpected: {BitConverter.ToString(bytes)}\nReceived: {BitConverter.ToString(inData)}");
                }
            }
        }

        [TestCase(true)]
        [TestCase(false)]
        public void TestPaddingText(bool forceNewTransform)
        {
            if (!forceNewTransform)
            {
                Assert.Ignore();
            }

            var length = 0;
            var algorithm = new NetAESEncryption(Peer)
            {
                ForceNewTransform = forceNewTransform
            };

            while (++length < LoremIpsum.Length)
            {
                var substring = LoremIpsum.Substring(0, length);
                var outMessage = Peer.CreateMessage();
                outMessage.Write(substring);

                var outBits = outMessage.LengthBits;

                var encryptSuccess = outMessage.Encrypt(algorithm);
                Assert.IsTrue(encryptSuccess, $"Failed to encrypt [length={length}]");

                var inMessage = outMessage.ToIncomingMessage();

                Assert.IsNotNull(inMessage.Data);
                Assert.IsNotEmpty(inMessage.Data);

                var decryptSuccess = inMessage.Decrypt(algorithm);
                Assert.IsTrue(decryptSuccess, "Failed to decrypt");
                Assert.IsNotNull(inMessage.Data);
                Assert.IsNotEmpty(inMessage.Data);
                Assert.AreEqual(outBits, inMessage.LengthBits);

                var inText = inMessage.ReadString();
                Assert.AreEqual(substring, inText, $"Expected '{substring}' received '{inText}'.");
            }
        }
    }
}
