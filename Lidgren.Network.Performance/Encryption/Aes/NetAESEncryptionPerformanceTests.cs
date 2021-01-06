using System;
using System.Diagnostics;

namespace Lidgren.Network.Performance.Encryption.Aes
{
    internal class NetAESEncryptionPerformanceTests
    {
        private const string LoremIpsum = "Lorem ipsum dolor sit amet, consectetur adipiscing elit. Proin viverra augue eget enim convallis ultricies. Vivamus rhoncus blandit nibh ut posuere. Aenean nec neque accumsan, egestas sem eu, molestie sem.";

        private NetPeerConfiguration Configuration { get; }

        private NetPeer Peer { get; }

        public NetAESEncryptionPerformanceTests()
        {
            Configuration = new NetPeerConfiguration(nameof(NetAESEncryptionPerformanceTests));
            Peer = new NetPeer(Configuration);
        }

        public void Run()
        {
            const int iterations = 10000;
            var partialTotal = MeasurePartial(iterations);
            var partialAverage = partialTotal.TotalMilliseconds / iterations;
            Console.WriteLine($"iterations={iterations} average={partialAverage}ms {partialTotal}ms");
            Debug.WriteLine($"iterations={iterations} average={partialAverage}ms {partialTotal}ms");
        }

        private TimeSpan MeasureFull(int iterations)
        {
            var elapsed = TimeSpan.Zero;

            for (var iteration = 0; iteration < iterations; ++iteration)
            {
                var length = 0;
                var algorithm = new NetAESEncryption(Peer);

                while (length++ < LoremIpsum.Length)
                {
                    var substring = LoremIpsum.Substring(0, length);
                    var outMessage = Peer.CreateMessage();
                    outMessage.Write(substring);

                    var outBits = outMessage.LengthBits;

                    var stopwatch = Stopwatch.StartNew();
                    var encryptSuccess = outMessage.Encrypt(algorithm);
                    stopwatch.Stop();
                    elapsed += stopwatch.Elapsed;
                    if (!encryptSuccess)
                    {
                        throw new Exception($"Failed to encrypt [length={length}]");
                    }

                    var inMessage = outMessage.ToIncomingMessage();

                    if ((inMessage?.Data?.Length ?? -1) < 1)
                    {
                        throw new Exception("Incoming message empty.");
                    }

                    var decryptSuccess = inMessage.Decrypt(algorithm);
                    if (!decryptSuccess)
                    {
                        throw new Exception($"Failed to decrypt");
                    }

                    if ((inMessage?.Data?.Length ?? -1) < 1)
                    {
                        throw new Exception("Incoming message empty.");
                    }

                    if (outBits != inMessage.LengthBits)
                    {
                        throw new Exception($"Expected {outBits}b, received {inMessage.LengthBits}");
                    }

                    var inText = inMessage.ReadString();
                    if (!string.Equals(substring, inText, StringComparison.Ordinal))
                    {
                        throw new Exception($"Expected '{substring}' received '{inText}'.");
                    }
                }
            }

            return elapsed;
        }

        private TimeSpan MeasurePartial(int iterations)
        {
            var elapsed = TimeSpan.Zero;

            for (var iteration = 0; iteration < iterations; ++iteration)
            {
                var length = 0;
                var algorithm = new NetAESEncryption(Peer);

                while (length++ < LoremIpsum.Length)
                {
                    var substring = LoremIpsum.Substring(0, length);
                    var outMessage = Peer.CreateMessage();
                    outMessage.Write(substring);

                    var outBits = outMessage.LengthBits;

                    var stopwatch = Stopwatch.StartNew();
                    var encryptSuccess = outMessage.Encrypt(algorithm);
                    stopwatch.Stop();
                    elapsed += stopwatch.Elapsed;
                    if (!encryptSuccess)
                    {
                        throw new Exception($"Failed to encrypt [length={length}]");
                    }

                    var inMessage = outMessage.ToIncomingMessage();

                    if ((inMessage?.Data?.Length ?? -1) < 1)
                    {
                        throw new Exception("Incoming message empty.");
                    }

                    var decryptSuccess = inMessage.Decrypt(algorithm);
                    if (!decryptSuccess)
                    {
                        throw new Exception($"Failed to decrypt");
                    }

                    if ((inMessage?.Data?.Length ?? -1) < 1)
                    {
                        throw new Exception("Incoming message empty.");
                    }

                    if (outBits != inMessage.LengthBits)
                    {
                        throw new Exception($"Expected {outBits}b, received {inMessage.LengthBits}");
                    }

                    var inText = inMessage.ReadString();
                    if (!string.Equals(substring, inText, StringComparison.Ordinal))
                    {
                        throw new Exception($"Expected '{substring}' received '{inText}'.");
                    }
                }
            }

            return elapsed;
        }
    }
}
