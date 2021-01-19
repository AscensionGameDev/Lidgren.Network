using NUnit.Framework;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Lidgren.Network.Tests.Peer
{
    [TestFixture]
    public class NetPeerShutdownTests : ServerTestFixture
    {
        public NetPeerShutdownTests() : base()
        {
            
        }

        [Test]
        public void TestShutdownMessage()
        {
            Assert.AreEqual(NetPeerStatus.Running, Peer.Status);
            Assert.IsNotNull(Connection);

            TestHelper.WaitForConnection(Connection);

            Assert.NotZero(Server.ConnectionsCount);

            var testString = $"{nameof(TestShutdownMessage)}_{new CryptoRandom().NextUInt64()}";
            var outgoingMessage = Peer.CreateMessage();
            outgoingMessage.Write(testString);
            Peer.Shutdown(outgoingMessage);

            TestHelper.WaitFor(() => Server.ConnectionsCount == 0);

            TestHelper.HasMessage(
                PeerMessages,
                NetIncomingMessageType.DebugMessage,
                message =>
                    string.Equals(
                        "Shutdown requested (reason)",
                        message.ReadString(),
                        StringComparison.Ordinal
                    )
            );

            var messageShutdownReason = ServerMessages.Last();
            Assert.AreEqual(NetIncomingMessageType.StatusChanged, messageShutdownReason.MessageType);
            var status = (NetConnectionStatus)messageShutdownReason.ReadByte();
            Assert.AreEqual(testString, messageShutdownReason.ReadString());
        }

        [Test]
        public void TestShutdownMessageWithDebugString()
        {
            Assert.AreEqual(NetPeerStatus.Running, Peer.Status);
            Assert.IsNotNull(Connection);

            TestHelper.WaitForConnection(Connection);

            Assert.NotZero(Server.ConnectionsCount);

            var testString = $"{nameof(TestShutdownMessage)}_{new CryptoRandom().NextUInt64()}";
            var outgoingMessage = Peer.CreateMessage();
            outgoingMessage.Write(testString);
            Peer.Shutdown(outgoingMessage, "debugMessage");

            TestHelper.WaitFor(() => Server.ConnectionsCount == 0);

            TestHelper.HasMessage(
                PeerMessages,
                NetIncomingMessageType.DebugMessage,
                message =>
                    string.Equals(
                        "Shutdown requested (debugMessage)",
                        message.ReadString(),
                        StringComparison.Ordinal
                    )
            );

            var messageShutdownReason = ServerMessages.Last();
            Assert.AreEqual(NetIncomingMessageType.StatusChanged, messageShutdownReason.MessageType);
            var status = (NetConnectionStatus)messageShutdownReason.ReadByte();
            Assert.AreEqual(testString, messageShutdownReason.ReadString());
        }

        [Test]
        public void TestShutdownString()
        {
            Assert.AreEqual(NetPeerStatus.Running, Peer.Status);
            Assert.IsNotNull(Connection);

            TestHelper.WaitForConnection(Connection);

            Assert.NotZero(Server.ConnectionsCount);

            Peer.Shutdown("bye");

            TestHelper.WaitFor(() => Server.ConnectionsCount == 0);

            TestHelper.HasMessage(
                PeerMessages,
                NetIncomingMessageType.DebugMessage,
                message =>
                    string.Equals(
                        "Shutdown requested (reason)",
                        message.ReadString(),
                        StringComparison.Ordinal
                    )
            );

            var messageShutdownReason = ServerMessages.Last();
            Assert.AreEqual(NetIncomingMessageType.StatusChanged, messageShutdownReason.MessageType);
            var status = (NetConnectionStatus)messageShutdownReason.ReadByte();
            Assert.AreEqual("bye", messageShutdownReason.ReadString());
        }

        [Test]
        public void TestShutdownStringWithDebugMessage()
        {
            Assert.AreEqual(NetPeerStatus.Running, Peer.Status);
            Assert.IsNotNull(Connection);

            TestHelper.WaitForConnection(Connection);

            Assert.NotZero(Server.ConnectionsCount);

            Peer.Shutdown("bye", "debugMessage");

            TestHelper.WaitFor(() => Server.ConnectionsCount == 0);

            TestHelper.HasMessage(
                PeerMessages,
                NetIncomingMessageType.DebugMessage,
                message =>
                    string.Equals(
                        "Shutdown requested (debugMessage)",
                        message.ReadString(),
                        StringComparison.Ordinal
                    )
            );

            var messageShutdownReason = ServerMessages.Last();
            Assert.AreEqual(NetIncomingMessageType.StatusChanged, messageShutdownReason.MessageType);
            var status = (NetConnectionStatus)messageShutdownReason.ReadByte();
            Assert.AreEqual("bye", messageShutdownReason.ReadString());
        }
    }
}
