using Lidgren.Network.Performance.Encryption.Aes;

using System;

namespace Lidgren.Network.Performance
{
    internal class Program
    {
        public static void Main()
        {
            try
            {
                new NetAESEncryptionPerformanceTests().Run();
            } catch
            {
                throw;
            }
        }
    }
}
