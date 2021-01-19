using System.Net;
using System.Net.Sockets;

namespace Lidgren.Network
{
    public interface INetSocket
    {
        int ReceiveBufferSize { get; set; }

        int SendBufferSize { get; set; }

        bool Blocking { get; set; }

        bool DualMode { get; set; }

        EndPoint LocalEndPoint { get; }

        bool IsBound { get; }

        int Available { get; }

        bool DontFragment { get; set; }

        void SetSocketOption(SocketOptionLevel optionLevel, SocketOptionName optionName, int value);

        void SetSocketOption(SocketOptionLevel optionLevel, SocketOptionName optionName, bool value);

        void Bind(EndPoint localEndpoint);

        void IOControl(int ioControlCode, byte[] optionInValue, byte[] optionOutValue);

        void Shutdown(SocketShutdown receive);

        void Close(int timeout);

        bool Poll(int microSeconds, SelectMode mode);

        int ReceiveFrom(byte[] buffer, int offset, int size, SocketFlags socketFlags, ref EndPoint remoteEndpoint);

        int SendTo(byte[] buffer, int offset, int size, SocketFlags socketFlags, EndPoint remoteEndpoint);
    }
}
