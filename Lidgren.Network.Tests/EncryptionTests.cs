using NUnit.Framework;
using System;
using static Lidgren.Network.Tests.TestHelper;

namespace Lidgren.Network.Tests
{
    [TestFixture]
    public class EncryptionTests : PeerTestFixture
    {
        [TestCase(typeof(NetXorEncryption), "TopSecret")]
        [TestCase(typeof(NetXtea), "TopSecret")]
        [TestCase(typeof(NetAESEncryption), "TopSecret")]
        [TestCase(typeof(NetRC2Encryption), "TopSecret")]
        [TestCase(typeof(NetDESEncryption), "TopSecret")]
        [TestCase(typeof(NetTripleDESEncryption), "TopSecret")]
        public void TestAlgorithm(Type netEncryptionType, string key)
        {
            var algo = Activator.CreateInstance(netEncryptionType, Peer, key) as NetEncryption;
            var om = Peer.CreateMessage();
            om.Write("Hallon");
            om.Write(42);
            om.Write(5, 5);
            om.Write(true);
            om.Write("kokos");
            int unencLen = om.LengthBits;
            om.Encrypt(algo);

            // convert to incoming message
            NetIncomingMessage im = CreateIncomingMessage(om.PeekDataBuffer(), om.LengthBits);
            if (im.Data == null || im.Data.Length == 0)
                Assert.Fail("bad im!");

            im.Decrypt(algo);

            if (im.Data == null || im.Data.Length == 0 || im.LengthBits != unencLen)
                Assert.Fail("Length fail");

            var str = im.ReadString();
            if (str != "Hallon")
                Assert.Fail("fail");

            if (im.ReadInt32() != 42)
                Assert.Fail("fail");

            if (im.ReadInt32(5) != 5)
                Assert.Fail("fail");

            if (im.ReadBoolean() != true)
                Assert.Fail("fail");

            if (im.ReadString() != "kokos")
                Assert.Fail("fail");
        }

        [TestCase(typeof(NetXorEncryption), "TopSecret")]
        [TestCase(typeof(NetXtea), "TopSecret")]
        [TestCase(typeof(NetAESEncryption), "TopSecret")]
        [TestCase(typeof(NetRC2Encryption), "TopSecret")]
        [TestCase(typeof(NetDESEncryption), "TopSecret")]
        [TestCase(typeof(NetTripleDESEncryption), "TopSecret")]
        public void TestEncryptorReuse(Type netEncryptionType, string key)
        {
            var algo = Activator.CreateInstance(netEncryptionType, Peer, key) as NetEncryption;
            for (var iteration = 0; iteration < 5; ++iteration)
            {
                var om = Peer.CreateMessage();
                om.Write("Hallon");
                om.Write(42);
                om.Write(5, 5);
                om.Write(true);
                om.Write("kokos");
                int unencLen = om.LengthBits;
                om.Encrypt(algo);

                // convert to incoming message
                NetIncomingMessage im = CreateIncomingMessage(om.PeekDataBuffer(), om.LengthBits);
                if (im.Data == null || im.Data.Length == 0)
                    Assert.Fail("bad im!");

                im.Decrypt(algo);

                if (im.Data == null || im.Data.Length == 0 || im.LengthBits != unencLen)
                    Assert.Fail("Length fail");

                var str = im.ReadString();
                if (str != "Hallon")
                    Assert.Fail("fail");

                if (im.ReadInt32() != 42)
                    Assert.Fail("fail");

                if (im.ReadInt32(5) != 5)
                    Assert.Fail("fail");

                if (im.ReadBoolean() != true)
                    Assert.Fail("fail");

                if (im.ReadString() != "kokos")
                    Assert.Fail("fail");
            }
        }

        [Test]
        [Repeat(100)]
        public void TestNetSRP()
        {
            byte[] salt = NetSRP.CreateRandomSalt();
            byte[] x = NetSRP.ComputePrivateKey("user", "password", salt);

            byte[] v = NetSRP.ComputeServerVerifier(x);
            //Console.WriteLine("v = " + NetUtility.ToHexString(v));

            byte[] a = NetSRP
                .CreateRandomEphemeral(); //  NetUtility.ToByteArray("393ed364924a71ba7258633cc4854d655ca4ec4e8ba833eceaad2511e80db2b5");

            byte[] A = NetSRP.ComputeClientEphemeral(a);
            //Console.WriteLine("A = " + NetUtility.ToHexString(A));

            byte[] b = NetSRP
                .CreateRandomEphemeral(); // NetUtility.ToByteArray("cc4d87a90db91067d52e2778b802ca6f7d362490c4be294b21b4a57c71cf55a9");

            byte[] B = NetSRP.ComputeServerEphemeral(b, v);
            //Console.WriteLine("B = " + NetUtility.ToHexString(B));

            byte[] u = NetSRP.ComputeU(A, B);
            //Console.WriteLine("u = " + NetUtility.ToHexString(u));

            byte[] Ss = NetSRP.ComputeServerSessionValue(A, v, u, b);
            //Console.WriteLine("Ss = " + NetUtility.ToHexString(Ss));

            byte[] Sc = NetSRP.ComputeClientSessionValue(B, x, u, a);
            //Console.WriteLine("Sc = " + NetUtility.ToHexString(Sc));

            if (Ss.Length != Sc.Length)
                Assert.Fail("SRP non matching lengths!");

            for (int j = 0; j < Ss.Length; j++)
            {
                if (Ss[j] != Sc[j])
                    Assert.Fail("SRP non matching session values!");
            }

            var test = NetSRP.CreateEncryption(Peer, Ss);
        }
    }
}