#if AES_GCM

using System.Buffers.Binary;
using System.Security.Cryptography;
using System.Text;

namespace Lidgren.Network.Encryption
{
    public class NetAesGcmEncryption : NetEncryption
    {
        public static void GenerateKey(out Span<byte> key) => GenerateKey(16, out key);

        public static void GenerateKey(int size, out Span<byte> key)
        {
            key = new byte[size].AsSpan();
            RandomNumberGenerator.Fill(key);
        }

        public static string DeriveKey(string source, string salt) => DeriveKey(source, salt, 16);

        public static string DeriveKey(string source, string salt, int keyLength)
        {
            var sourceBytes = Encoding.UTF8.GetBytes(source);
            var saltBytes = Convert.FromBase64String(salt);
            var keyBytes = DeriveKey(sourceBytes, saltBytes, keyLength);
            return Convert.ToBase64String(keyBytes);
        }

        public static string DeriveKey(string source, out string salt) => DeriveKey(source, 16, out salt);

        public static string DeriveKey(string source, int keyLength, out string salt)
        {
            var sourceBytes = Encoding.UTF8.GetBytes(source);
            var keyBytes = DeriveKey(sourceBytes, keyLength, out var saltBytes);
            salt = Convert.ToBase64String(saltBytes);
            return Convert.ToBase64String(keyBytes);
        }

        public static Span<byte> DeriveKey(byte[] source, byte[] salt) => DeriveKey(source, salt, 16);

        public static Span<byte> DeriveKey(byte[] source, byte[] salt, int keyLength)
        {
            using var deriveBytes = new Rfc2898DeriveBytes(source, salt, 1000, HashAlgorithmName.SHA512);
            var key = deriveBytes.GetBytes(keyLength);
            return key.AsSpan();
        }

        public static Span<byte> DeriveKey(byte[] source, out byte[] salt) => DeriveKey(source, 16, out salt);

        public static Span<byte> DeriveKey(byte[] source, int keyLength, out byte[] salt)
        {
            salt = new byte[16];
            RandomNumberGenerator.Fill(salt.AsSpan());
            return DeriveKey(source, salt, keyLength);
        }

        private AesGcm _aesGcm { get; set; }

        public NetAesGcmEncryption(NetPeer peer, in byte[] key) : base(peer)
        {
            if (default != key)
            {
                _aesGcm = new AesGcm(key);
            }
        }

        public NetAesGcmEncryption(NetPeer peer, in ReadOnlySpan<byte> key) : base(peer)
        {
            if (default != key)
            {
                _aesGcm = new AesGcm(key);
            }
        }

        private const int NonceSize = 12;
        private const int TagSize = 16;

        public override bool Decrypt(NetIncomingMessage msg)
        {
            try
            {
                var ciphertextOffset = 0;// msg.PositionInBytes;
                var remaining = msg.LengthBytes - ciphertextOffset;

                if (remaining < (sizeof(int) * 3))
                {
                    return false;
                }

                var cipherData = msg.m_data.AsSpan()[ciphertextOffset..];

                var offset = 0;

                var bitLength = BinaryPrimitives.ReadInt32LittleEndian(cipherData.Slice(offset, sizeof(int)));
                offset += sizeof(int);

                var nonceLength = cipherData.Slice(offset, sizeof(int))[0];
                offset += sizeof(byte);

                var tagLength = cipherData.Slice(offset, sizeof(byte))[0];
                offset += sizeof(byte);

                var nonce = cipherData.Slice(offset, nonceLength);
                offset += nonceLength;

                var ciphertextLength = (bitLength + 7) >> 3;
                var ciphertext = cipherData.Slice(offset, ciphertextLength);
                offset += ciphertextLength;

                var tag = cipherData.Slice(offset, tagLength);
                offset += tagLength;

                byte[]? fromPool = default;
                var plaintext = (ciphertextLength > 1024 ? (fromPool = m_peer.GetStorage(ciphertextLength)) : stackalloc byte[ciphertextLength])[0..ciphertextLength];

                _aesGcm.Decrypt(nonce, ciphertext, tag, plaintext, null);

                plaintext.CopyTo(cipherData);
                cipherData[plaintext.Length..offset].Fill(0);

                if (fromPool != default)
                {
                    m_peer.Recycle(fromPool);
                }

                msg.m_bitLength = bitLength;

                return true;
            }
            catch
            {
                throw;
            }
        }

        public override bool Encrypt(NetOutgoingMessage msg)
        {
            try
            {
                var plaintextOffset = 0;//msg.PositionInBytes
                var plaintextLength = msg.LengthBytes - plaintextOffset;

                if (plaintextLength < 1)
                {
                    return true;
                }

                var plaintext = msg.m_data.AsSpan()[plaintextOffset..msg.LengthBytes];
                var headerLength = sizeof(int) * 3;
                var cipherLength = headerLength + NonceSize + TagSize + plaintextLength;

                byte[]? fromPool = default;
                var data = (cipherLength > 1024 ? (fromPool = m_peer.GetStorage(cipherLength)) : stackalloc byte[cipherLength])[0..cipherLength];

                var offset = 0;

                BinaryPrimitives.WriteInt32LittleEndian(data.Slice(offset, sizeof(int)), msg.m_bitLength);// - msg.m_readPosition);
                offset += sizeof(int);

                data.Slice(offset, sizeof(byte))[0] = (byte)(NonceSize & 0xff);
                offset += sizeof(byte);

                data.Slice(offset, sizeof(byte))[0] = (byte)(TagSize & 0xff);
                offset += sizeof(byte);

                var nonce = data.Slice(offset, NonceSize);
                offset += NonceSize;

                var ciphertext = data[offset..(offset + plaintextLength)];
                offset += ciphertext.Length;

                var tag = data.Slice(offset, TagSize);

                RandomNumberGenerator.Fill(nonce);

                _aesGcm.Encrypt(nonce, plaintext, ciphertext, tag, null);

                m_peer.Recycle(msg.m_data);
                byte[]? newBuffer = fromPool;
                if (newBuffer == default)
                {
                    newBuffer = m_peer.GetStorage(data.Length);
                    data.CopyTo(newBuffer.AsSpan());
                }
                msg.m_data = newBuffer;
                msg.m_bitLength = data.Length << 3;
                msg.m_readPosition = msg.m_bitLength;

                return true;
            }
            catch
            {
                throw;
            }
        }

        public override void SetKey(byte[] data, int offset, int count) =>
            _aesGcm = new AesGcm(new ReadOnlySpan<byte>(data, offset, count));
    }
}

#endif
