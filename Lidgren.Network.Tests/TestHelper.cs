using NUnit.Framework;

using System;
using System.Collections.Generic;
using System.Linq;
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

        public static NetIncomingMessage ToIncomingMessage(
            this NetOutgoingMessage outgoingMessage,
            NetIncomingMessageType incomingMessageType = NetIncomingMessageType.UnconnectedData
        ) =>
            new NetIncomingMessage
            {
                m_incomingMessageType = incomingMessageType,
                m_data = outgoingMessage.m_data.ToArray(),
                m_bitLength = outgoingMessage.m_bitLength
            };

        public static void WaitForConnection(NetConnection connection, int failureTimeout = 10000)
        {
            var interval = Math.Max(1, failureTimeout / 100);
            var currentTime = 0;
            while (currentTime < failureTimeout)
            {
                switch (connection.Status)
                {
                    case NetConnectionStatus.None:
                    case NetConnectionStatus.InitiatedConnect:
                        NetUtility.Sleep(interval);
                        currentTime += interval;
                        break;

                    case NetConnectionStatus.Connected:
                        return;

                    default:
                        Assert.Fail($"Connection failed, current status: {connection.Status}");
                        break;
                }
            }

            Assert.Fail($"Connection not established within {failureTimeout}ms, current status: {connection.Status}");
        }

        public static void WaitFor(Func<bool> condition, int failureTimeout = 10000)
        {
            var interval = Math.Max(1, failureTimeout / 100);
            var currentTime = 0;
            while (currentTime < failureTimeout)
            {
                if (condition())
                {
                    return;
                }

                NetUtility.Sleep(interval);
                currentTime += interval;
            }

            Assert.Fail($"Condition not met within {failureTimeout}ms");
        }

        public static void HasMessage(
            IList<NetIncomingMessage> messages,
            NetIncomingMessageType messageType,
            Func<NetIncomingMessage, bool> filter = default
        )
        {
            var hasMessage = messages.Any(
                message =>
                    message.MessageType == messageType &&
                    (filter == default || filter(message))
            );

            Assert.IsTrue(hasMessage);
        }
    }
}