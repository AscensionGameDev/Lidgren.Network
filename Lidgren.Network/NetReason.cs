using System;

namespace Lidgren.Network
{
    public struct NetReason
    {
        public static readonly NetReason Empty = default;

        public NetOutgoingMessage Message { get; }

        public string Text { get; }

        public int Length => Message?.LengthBytes ?? (4 + Text.Length + (Text.Length > 126 ? 2 : 1));

        public NetReason(string text)
        {
            Message = null;
            Text = text ?? throw new ArgumentNullException(nameof(text));
        }

        public NetReason(NetOutgoingMessage message)
        {
            Message = message ?? throw new ArgumentNullException(nameof(message));
            Text = null;
        }

        public static implicit operator NetReason(string text) =>
            new NetReason(text);

        public static implicit operator NetReason(NetOutgoingMessage message) =>
            new NetReason(message);
    }
}
