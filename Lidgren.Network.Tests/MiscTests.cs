using NUnit.Framework;

namespace Lidgren.Network.Tests
{
    [TestFixture]
    public class MiscTests
    {
        [Test]
        public void TestEnableMessageType()
        {
            var config = new NetPeerConfiguration("Test");
            config.EnableMessageType(NetIncomingMessageType.UnconnectedData);
            Assert.IsTrue(config.IsMessageTypeEnabled(NetIncomingMessageType.UnconnectedData), "setting enabled message types failed");
        }

        [Test]
        public void TestSetMessageTypeEnabled()
        {
            var config = new NetPeerConfiguration("Test");
            config.SetMessageTypeEnabled(NetIncomingMessageType.UnconnectedData, false);
            Assert.IsFalse(config.IsMessageTypeEnabled(NetIncomingMessageType.UnconnectedData), "setting enabled message types failed");
        }

        [Test]
        public void TestToHexString() => Assert.AreEqual("DEADBEEF", NetUtility.ToHexString(new byte[] { 0xDE, 0xAD, 0xBE, 0xEF }));

        [Test]
        public void TestBitsToHoldUInt64()
        {
            Assert.AreEqual(33, NetUtility.BitsToHoldUInt64((ulong)uint.MaxValue + 1ul), "BitsToHoldUInt64 failed");
        }
    }
}