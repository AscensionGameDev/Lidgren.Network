using System;
using System.Reflection;

namespace Lidgren.Network.Tests
{
    public static class TestHelper
    {
        /// <summary>
        /// Helper method
        /// </summary>
        public static NetIncomingMessage CreateIncomingMessage(byte[] fromData, int bitLength)
        {
            var inc = (NetIncomingMessage)Activator.CreateInstance(typeof(NetIncomingMessage), true);
            typeof(NetIncomingMessage).GetField("m_data", BindingFlags.NonPublic | BindingFlags.Instance)
                .SetValue(inc, fromData);

            typeof(NetIncomingMessage).GetField("m_bitLength", BindingFlags.NonPublic | BindingFlags.Instance)
                .SetValue(inc, bitLength);

            return inc;
        }
    }
}