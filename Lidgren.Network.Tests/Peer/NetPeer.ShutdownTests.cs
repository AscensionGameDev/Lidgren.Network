using NUnit.Framework;

using System;
using System.Linq;

namespace Lidgren.Network.Tests.Peer
{
    [TestFixture]
    public class NetPeerShutdownTests : ServerTestFixture
    {
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

            var messageShutdownReason = ServerMessages.Last(
                message => NetIncomingMessageType.StatusChanged == message.MessageType
                );
            Assert.IsNotNull(messageShutdownReason);
            var status = (NetConnectionStatus)messageShutdownReason.ReadByte();
            Assert.AreEqual(NetConnectionStatus.Disconnected, status);
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

            var messageShutdownReason = ServerMessages.Last(
                message => NetIncomingMessageType.StatusChanged == message.MessageType
                );
            Assert.IsNotNull(messageShutdownReason);
            var status = (NetConnectionStatus)messageShutdownReason.ReadByte();
            Assert.AreEqual(NetConnectionStatus.Disconnected, status);
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

            var messageShutdownReason = ServerMessages.Last(
                message => NetIncomingMessageType.StatusChanged == message.MessageType
                );
            Assert.IsNotNull(messageShutdownReason);
            var status = (NetConnectionStatus)messageShutdownReason.ReadByte();
            Assert.AreEqual(NetConnectionStatus.Disconnected, status);
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

            var messageShutdownReason = ServerMessages.Last(
                message => NetIncomingMessageType.StatusChanged == message.MessageType
                );
            Assert.IsNotNull(messageShutdownReason);
            var status = (NetConnectionStatus)messageShutdownReason.ReadByte();
            Assert.AreEqual(NetConnectionStatus.Disconnected, status);
            Assert.AreEqual("bye", messageShutdownReason.ReadString());
        }
    }
}
