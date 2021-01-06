using System.Linq;

namespace Lidgren.Network.Performance
{
    internal static class Helpers
    {
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
    }
}
