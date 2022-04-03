#if AES_GCM

using Lidgren.Network.Encryption;

using NUnit.Framework;

using System;
using System.Buffers.Binary;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace Lidgren.Network.Tests.Encryption
{
    [TestFixture]
    public class AesGcmTests : PeerTestFixture
    {
        private const string LoremIpsum = "Lorem ipsum dolor sit amet, consectetur adipiscing elit. Proin viverra augue eget enim convallis ultricies. Vivamus rhoncus blandit nibh ut posuere. Aenean nec neque accumsan, egestas sem eu, molestie sem.";
        private const string LoremIpsumShort = "Lorem ipsum dolor sit amet";

        [Test]
        public void TestBytes()
        {
            NetAesGcmEncryption.GenerateKey(out var key);
            var algorithm = new NetAesGcmEncryption(Peer, key);

            var bytes = Encoding.UTF8.GetBytes(LoremIpsum);
            var length = bytes.Length;
            var outMessage = Peer.CreateMessage();
            outMessage.Write(bytes, 0, length);

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

            var inData = inMessage.PeekDataBuffer();
            for (var index = 0; index < length; ++index)
            {
                Assert.AreEqual(bytes[index], inData[index], $"Arrays differed at index {index} (out of {length}).\nExpected: {BitConverter.ToString(bytes)}\nReceived: {BitConverter.ToString(inData)}");
            }
        }

        [Test]
        public void TestTextPreconfigured()
        {
            var password = "test1234";
            var salt = Convert.ToBase64String(new byte[16].Select((_, i) => (byte)i).ToArray());
            var keyBase64 = NetAesGcmEncryption.DeriveKey(password, salt, 32);
            var key = Convert.FromBase64String(keyBase64);
            var algorithm = new NetAesGcmEncryption(Peer, key);

            var nonce = new byte[12].Select((_, i) => (byte)i).ToArray().AsSpan();
            var inMessage = Peer.CreateMessage(3 * sizeof(int) + 12 + 16 + 27).ToIncomingMessage();
            inMessage.LengthBytes = 67;
            var inBufferSpan = inMessage.PeekDataBuffer().AsSpan();
            BinaryPrimitives.WriteInt32LittleEndian(inBufferSpan[0..], 216);
            BinaryPrimitives.WriteInt32LittleEndian(inBufferSpan[4..], 12);
            BinaryPrimitives.WriteInt32LittleEndian(inBufferSpan[8..], 16);
            nonce.CopyTo(inBufferSpan[12..]);
            var expectedCipherTextAndTag = "ECxGVlhfHY5YMNY95cORVvOl12140/YiRwFZu2M5HsFzYxyxZsGKGzyxqg==";
            var expectedCipherDataAndTag = Convert.FromBase64String(expectedCipherTextAndTag);
            var expectedCipherData = expectedCipherDataAndTag.AsSpan()[0..27];
            expectedCipherDataAndTag.CopyTo(inBufferSpan[24..]);
            inMessage.LengthBytes = 67;

            Assert.IsNotNull(inMessage.Data);
            Assert.IsNotEmpty(inMessage.Data);

            var decryptSuccess = inMessage.Decrypt(algorithm);
            Assert.IsTrue(decryptSuccess, "Failed to decrypt");
            Assert.IsNotNull(inMessage.Data);
            Assert.IsNotEmpty(inMessage.Data);
            Assert.AreEqual(216, inMessage.LengthBits);

            var inText = inMessage.ReadString();
            Assert.AreEqual(LoremIpsumShort, inText, $"Expected '{LoremIpsumShort}' received '{inText}'.");
        }

        [Test]
        public void TestText()
        {
            NetAesGcmEncryption.GenerateKey(out var key);
            var algorithm = new NetAesGcmEncryption(Peer, key);

            var outMessage = Peer.CreateMessage();
            outMessage.Write(LoremIpsumShort);

            var outBits = outMessage.LengthBits;

            var encryptSuccess = outMessage.Encrypt(algorithm);
            Assert.IsTrue(encryptSuccess, $"Failed to encrypt [length={LoremIpsumShort.Length}]");

            var inMessage = outMessage.ToIncomingMessage();

            Assert.IsNotNull(inMessage.Data);
            Assert.IsNotEmpty(inMessage.Data);

            var decryptSuccess = inMessage.Decrypt(algorithm);
            Assert.IsTrue(decryptSuccess, "Failed to decrypt");
            Assert.IsNotNull(inMessage.Data);
            Assert.IsNotEmpty(inMessage.Data);
            Assert.AreEqual(outBits, inMessage.LengthBits);

            var inText = inMessage.ReadString();
            Assert.AreEqual(LoremIpsumShort, inText, $"Expected '{LoremIpsumShort}' received '{inText}'.");
        }

        [Test]
        public void TestPaddingBytes()
        {
            NetAesGcmEncryption.GenerateKey(out var key);
            var algorithm = new NetAesGcmEncryption(Peer, key);

            var bytes = Encoding.UTF8.GetBytes(LoremIpsum);
            var length = 0;

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

        [Test]
        public void TestPaddingText()
        {
            NetAesGcmEncryption.GenerateKey(out var key);
            var algorithm = new NetAesGcmEncryption(Peer, key);

            var length = 0;

            while (length++ < LoremIpsum.Length)
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

#endif
