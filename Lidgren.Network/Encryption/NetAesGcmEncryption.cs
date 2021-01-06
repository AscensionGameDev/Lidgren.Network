#if AES_GCM

using System;
using System.Buffers.Binary;
using System.Security.Cryptography;

namespace Lidgren.Network.Encryption
{
    public class NetAesGcmEncryption : NetEncryption
    {
        public static Span<byte> GenerateKey()
        {
            var key = new byte[16].AsSpan();
            RandomNumberGenerator.Fill(key);
            return key;
        }

        public static Span<byte> DeriveKey(string source) =>
            new Rfc2898DeriveBytes(source, 16, 1000, HashAlgorithmName.SHA512).GetBytes(16);

        public static Span<byte> DeriveKey(byte[] source)
        {
            var salt = new byte[16];
            RandomNumberGenerator.Fill(salt.AsSpan());
            return new Rfc2898DeriveBytes(source, salt, 1000, HashAlgorithmName.SHA512).GetBytes(16);
        }

        private AesGcm _aesGcm { get; set; }

        public NetAesGcmEncryption(NetPeer peer)
            : this(peer, GenerateKey())
        {
        }

        public NetAesGcmEncryption(NetPeer peer, byte[] key) : base(peer)
        {
            if (default != key)
            {
                _aesGcm = new AesGcm(key);
            }
        }

        public NetAesGcmEncryption(NetPeer peer, ReadOnlySpan<byte> key) : base(peer)
        {
            if (default != key)
            {
                _aesGcm = new AesGcm(key);
            }
        }

        public override bool Decrypt(NetIncomingMessage msg)
        {
            var sourceData = msg.m_data.AsSpan();
            var data = sourceData[msg.PositionInBytes..].ToArray().AsSpan();

            var offset = 0;
            
            var bitLength = BinaryPrimitives.ReadInt32LittleEndian(data.Slice(offset, sizeof(int)));
            offset += sizeof(int);
            
            var nonceLength = BinaryPrimitives.ReadInt32LittleEndian(data.Slice(offset, sizeof(int)));
            offset += sizeof(int);
            
            var tagLength = BinaryPrimitives.ReadInt32LittleEndian(data.Slice(offset, sizeof(int)));
            offset += sizeof(int);

            var nonce = data.Slice(offset, nonceLength);
            offset += nonceLength;

            var tag = data.Slice(offset, tagLength);
            offset += tagLength;

            var ciphertext = data[offset..];

            var plaintext = sourceData.Slice(msg.PositionInBytes, ciphertext.Length);
            sourceData[(msg.PositionInBytes + ciphertext.Length)..].Fill(0);

            _aesGcm.Decrypt(nonce, ciphertext, tag, plaintext, null);

            msg.m_bitLength = bitLength;
            msg.m_readPosition = 0;

            return true;
        }
        
        public override bool Encrypt(NetOutgoingMessage msg)
        {
            var plaintext = msg.m_data.AsSpan()[msg.PositionInBytes..];
            
            var nonceLength = AesGcm.NonceByteSizes.MaxSize;
            var tagLength = AesGcm.TagByteSizes.MaxSize;
            var cipherLength = plaintext.Length;
            var dataBuffer = new byte[sizeof(int) * 3 + nonceLength + tagLength + cipherLength];
            var data = dataBuffer.AsSpan();

            var offset = 0;
            
            BinaryPrimitives.WriteInt32LittleEndian(data.Slice(offset, sizeof(int)), msg.m_bitLength);
            offset += sizeof(int);

            BinaryPrimitives.WriteInt32LittleEndian(data.Slice(offset, sizeof(int)), nonceLength);
            offset += sizeof(int);
            
            BinaryPrimitives.WriteInt32LittleEndian(data.Slice(offset, sizeof(int)), tagLength);
            offset += sizeof(int);

            var nonce = data.Slice(offset, nonceLength);
            offset += nonceLength;

            var tag = data.Slice(offset, tagLength);
            offset += tagLength;

            var ciphertext = data[offset..];

            RandomNumberGenerator.Fill(nonce);

            _aesGcm.Encrypt(nonce, plaintext, ciphertext, tag, null);

            m_peer.Recycle(msg.m_data);
            msg.m_data = dataBuffer;
            msg.m_bitLength = dataBuffer.Length << 3;
            msg.m_readPosition = msg.m_bitLength;

            return true;
        }

        public override void SetKey(byte[] data, int offset, int count) =>
            _aesGcm = new AesGcm(new ReadOnlySpan<byte>(data, offset, count));
    }
}

#endif
