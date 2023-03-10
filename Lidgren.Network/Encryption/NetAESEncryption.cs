using System;
using System.Reflection;
using System.Runtime.Versioning;
using System.Security.Cryptography;

namespace Lidgren.Network
{
    public class NetAESEncryption : NetCryptoProviderBase
    {

        static NetAESEncryption()
        {
#if !UNITY
            var frameworkName = Assembly.GetEntryAssembly()
                ?.GetCustomAttribute<TargetFrameworkAttribute>()
                ?.FrameworkName;
            if (frameworkName?.Contains("v4.6.2") ?? false)
            {
                AppContext.SetSwitch("Switch.System.Security.Cryptography.AesCryptoServiceProvider.DontCorrectlyResetDecryptor", false);
            }
#endif
        }

        public NetAESEncryption(NetPeer peer)
#if UNITY
			: base(peer, new RijndaelManaged())
#else
            : base(peer, new AesCryptoServiceProvider
            {
                Mode = CipherMode.CBC,
                Padding = PaddingMode.PKCS7
            })
#endif
        {
        }

        public NetAESEncryption(NetPeer peer, string key)
            : this(peer)
        {
            SetKey(key);
        }

        public NetAESEncryption(NetPeer peer, byte[] data) : this(peer, data, 0, data.Length) { }

        public NetAESEncryption(NetPeer peer, byte[] data, int offset, int count)
            : this(peer)
        {
            SetKey(data, offset, count);
        }
    }
}
