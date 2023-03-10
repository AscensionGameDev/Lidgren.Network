namespace Lidgren.Network.Encryption
{

#if DEBUG

    public class NetNoopEncryption : NetEncryption
    {
        public NetNoopEncryption(NetPeer peer, params object[] _args) : base(peer)
        {
        }

        public override bool Decrypt(NetIncomingMessage msg) => true;

        public override bool Encrypt(NetOutgoingMessage msg) => true;

        public override void SetKey(byte[] data, int offset, int count) { }
    }

#endif
    
}
