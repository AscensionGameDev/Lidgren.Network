using System;
using System.Collections.Concurrent;
#if DEBUG
using System.Diagnostics;
#endif
using System.IO;
using System.Security.Cryptography;

namespace Lidgren.Network
{
    public abstract class NetCryptoProviderBase : NetEncryption
	{
		protected sealed class ICryptoTransformProvider
        {
			public delegate ICryptoTransform CreateTransform();

			private readonly NetCryptoProviderBase m_cryptoProvider;
			private readonly CreateTransform m_createTransform;
			private readonly ConcurrentStack<ICryptoTransform> m_transforms;

			public ICryptoTransformProvider(
				NetCryptoProviderBase cryptoProvider,
				CreateTransform createTransform
			)
            {
				m_cryptoProvider = cryptoProvider;
				m_createTransform = createTransform;
				m_transforms = new ConcurrentStack<ICryptoTransform>();
            }

			public bool Pop(out ICryptoTransform cryptoTransform)
            {
				if (m_cryptoProvider.ForceNewTransform || !m_transforms.TryPop(out cryptoTransform))
                {
					cryptoTransform = m_createTransform();
				}

				return true;
            }

			public void Push(ICryptoTransform cryptoTransform)
            {
				if (default == cryptoTransform)
                {
					throw new ArgumentNullException(nameof(cryptoTransform));
                }

				if (!m_cryptoProvider.ForceNewTransform && cryptoTransform.CanReuseTransform)
                {
					m_transforms.Push(cryptoTransform);
				} else
                {
					cryptoTransform.Dispose();
                }
			}
        }

		protected readonly SymmetricAlgorithm m_algorithm;
		protected readonly ICryptoTransformProvider m_encryptorProvider;
		protected readonly ICryptoTransformProvider m_decryptorProvider;

		public bool ForceNewTransform { get; set; }

#if DEBUG
		internal byte[] Key => m_algorithm.Key;

		internal byte[] IV => m_algorithm.IV;

		internal int KeySize => m_algorithm.KeySize;

		internal int BlockSize => m_algorithm.BlockSize;
#endif

		public NetCryptoProviderBase(NetPeer peer, SymmetricAlgorithm algo)
			: base(peer)
		{
			m_algorithm = algo;
			m_encryptorProvider = new ICryptoTransformProvider(this, m_algorithm.CreateEncryptor);
			m_decryptorProvider = new ICryptoTransformProvider(this, m_algorithm.CreateDecryptor);
			m_algorithm.GenerateKey();
			m_algorithm.GenerateIV();
		}

		public override void SetKey(byte[] data, int offset, int count)
		{
			int len = m_algorithm.Key.Length;
			var key = new byte[len];
			for (int i = 0; i < len; i++)
				key[i] = data[offset + (i % count)];
			m_algorithm.Key = key;

			len = m_algorithm.IV.Length;
			key = new byte[len];
			for (int i = 0; i < len; i++)
				key[len - 1 - i] = data[offset + (i % count)];
			m_algorithm.IV = key;
		}

		public override bool Encrypt(NetOutgoingMessage msg)
		{
			try
			{
				var plaintextBits = msg.LengthBits;
				var blocks = (int)Math.Ceiling(plaintextBits / (double)m_algorithm.BlockSize);
				var paddedLengthBytes = sizeof(int) + blocks * m_algorithm.BlockSize / 8;
				var memoryStream = new MemoryStream(paddedLengthBytes);
				memoryStream.Write(BitConverter.GetBytes(plaintextBits), 0, sizeof(int));

				if (!m_encryptorProvider.Pop(out var cryptoTransform))
				{
					return false;
				}

				var cs = new CryptoStream(memoryStream, cryptoTransform, CryptoStreamMode.Write);
				cs.Write(msg.m_data, 0, msg.LengthBytes);
				cs.Close();

				// Recycle existing data buffer
				m_peer.Recycle(msg.m_data);

				// get results
				msg.m_bitLength = paddedLengthBytes << 3;
				msg.m_readPosition = msg.m_bitLength;
				msg.m_data = memoryStream.GetBuffer();
				memoryStream.Close();

				return true;
			}
			catch
			{
#if DEBUG
				Debugger.Break();
#endif
				throw;
			}
		}

		public override bool Decrypt(NetIncomingMessage msg)
		{
			try
			{
				var memoryStream = new MemoryStream(msg.m_data);
				var plaintextBitsBuffer = new byte[sizeof(int)];
				if (sizeof(int) != memoryStream.Read(plaintextBitsBuffer, 0, sizeof(int)))
                {
					throw new Exception();
                }

				var plaintextBits = BitConverter.ToInt32(plaintextBitsBuffer, 0);

				if (!m_decryptorProvider.Pop(out var cryptoTransform))
				{
					return false;
				}

				var cs = new CryptoStream(memoryStream, cryptoTransform, CryptoStreamMode.Read);

				var byteLen = NetUtility.BytesToHoldBits(plaintextBits);
				var result = m_peer.GetStorage(byteLen << 3);

                var read = 0;
                while (read < byteLen)
                {
                    var cRead = cs.Read(result, read, byteLen - read);
                    if (cRead == 0)
                        return false;

                    read += cRead;
                }

                cs.Close();

				// Recycle existing data buffer
				m_peer.Recycle(msg.m_data);

				msg.m_data = result;
				msg.m_bitLength = plaintextBits;
				msg.m_readPosition = 0;

				return true;
			} catch
			{
#if DEBUG
				Debugger.Break();
#endif
				throw;
			}
		}
	}
}
