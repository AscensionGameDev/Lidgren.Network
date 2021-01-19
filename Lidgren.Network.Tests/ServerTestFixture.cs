using System.Collections.Generic;
using System.Diagnostics;
using System.Net;
using System.Threading;
using System.Threading.Tasks;

namespace Lidgren.Network.Tests
{
    public abstract class ServerTestFixture : PeerTestFixture<NetClient>
    {
        protected NetServer Server { get; private set; }

        protected Thread ServerMessageHandlerThread { get; private set; }

        protected NetConnection Connection { get; private set; }

        protected NetPeerConfiguration ServerConfiguration { get; set; }

        public List<NetIncomingMessage> ServerMessages { get; private set; }

        protected override bool StartPeer => true;

        protected ServerTestFixture(string appIdentifier = null) : base(appIdentifier)
        {
            ServerConfiguration = new NetPeerConfiguration(AppIdentifier)
            {
                LocalAddress = IPAddress.Loopback,
                Port = 0,
                EnableUPnP = false,
                AcceptIncomingConnections = true
            };
        }

        protected override NetClient CreatePeer(NetPeerConfiguration peerConfiguration) =>
            new NetClient(peerConfiguration);

        public override void SetUp()
        {
            base.SetUp();

            Server = new NetServer(ServerConfiguration);

            ServerMessages?.Clear();
            ServerMessages = new List<NetIncomingMessage>();

            ServerMessageHandlerThread = CreateMessageHandlerThread(Server, HandleServerMessage);

            Server.Start();
            ServerMessageHandlerThread.Start();

            Connection = Peer.Connect(new IPEndPoint(IPAddress.Loopback, Server.Port));
        }

        public virtual void HandleServerMessage(NetIncomingMessage incomingMessage)
        {
            switch (incomingMessage.MessageType)
            {
                case NetIncomingMessageType.DebugMessage:
                case NetIncomingMessageType.VerboseDebugMessage:
                case NetIncomingMessageType.WarningMessage:
                case NetIncomingMessageType.Error:
                case NetIncomingMessageType.ErrorMessage:
                case NetIncomingMessageType.StatusChanged:
                case NetIncomingMessageType.Data:
                case NetIncomingMessageType.UnconnectedData:
                    ServerMessages.Add(incomingMessage);
                    break;

                default:
                    Debugger.Break();
                    break;
            }
        }

        public override void TearDown()
        {
            base.TearDown();

            Server?.Shutdown(string.Empty);
        }
    }
}
