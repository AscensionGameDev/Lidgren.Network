using NUnit.Framework;

namespace Lidgren.Network.Tests
{
    [TestFixture]
    public abstract class PeerTestFixture
    {
        protected string AppIdentifier { get; }

        protected NetPeerConfiguration Configuration { get; private set; }

        protected NetPeer Peer { get; private set; }

        protected PeerTestFixture() : this(null)
        {
        }

        protected PeerTestFixture(string appIdentifier)
        {
            AppIdentifier = string.IsNullOrWhiteSpace(appIdentifier) ? GetType().FullName : appIdentifier;
        }

        [SetUp]
        public virtual void SetUp()
        {
            Configuration = new NetPeerConfiguration(AppIdentifier)
            {
                EnableUPnP = true
            };

            Peer = new NetPeer(Configuration);
        }

        [TearDown]
        public virtual void TearDown() => Peer?.Shutdown("bye");
    }
}