using NUnit.Framework;

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;

namespace Lidgren.Network.Tests
{
    [TestFixture]
    public abstract class PeerTestFixture<TPeer> where TPeer : NetPeer
    {
        protected string AppIdentifier { get; }

        protected NetPeerConfiguration Configuration { get; set; }

        protected TPeer Peer { get; private set; }

        protected Thread PeerMessageHandlerThread { get; private set; }

        protected virtual bool StartPeer => false;

        public List<NetIncomingMessage> PeerMessages { get; private set; }

        protected PeerTestFixture() : this(null)
        {
        }

        protected PeerTestFixture(string appIdentifier)
        {
            AppIdentifier = string.IsNullOrWhiteSpace(appIdentifier) ? GetType().FullName : appIdentifier;
            Configuration = new NetPeerConfiguration(AppIdentifier)
            {
                EnableUPnP = true
            };
        }

        protected abstract TPeer CreatePeer(NetPeerConfiguration peerConfiguration);

        protected virtual Thread CreateMessageHandlerThread(NetPeer netPeer, Action<NetIncomingMessage> handler)
        {
            var thread = new Thread(() =>
            {
                while (netPeer?.Status != NetPeerStatus.NotRunning)
                {
                    while (netPeer.ReadMessage(out var incomingMessage))
                    {
                        handler(incomingMessage);
                    }
                }
            });

            return thread;
        }

        public virtual void HandlePeerMessage(NetIncomingMessage incomingMessage)
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
                    PeerMessages.Add(incomingMessage);
                    break;

                default:
                    Debugger.Break();
                    break;
            }
        }

        [SetUp]
        public virtual void SetUp()
        {
            Peer = CreatePeer(Configuration);

            PeerMessages?.Clear();
            PeerMessages = new List<NetIncomingMessage>();
            Configuration = new NetPeerConfiguration(AppIdentifier);

            if (StartPeer)
            {
                PeerMessageHandlerThread = CreateMessageHandlerThread(Peer, HandlePeerMessage);

                Peer.Start();
                PeerMessageHandlerThread.Start();
            }
        }

        [TearDown]
        public virtual void TearDown()
        {
            Peer?.Shutdown("bye");
        }
    }

    public abstract class PeerTestFixture : PeerTestFixture<NetPeer>
    {
        protected override NetPeer CreatePeer(NetPeerConfiguration peerConfiguration) =>
            new NetPeer(peerConfiguration);
    }
}
