using System.Net;
using System.Net.Sockets;

public static class NetworkHelper 
{
    public const string HOST = "8.8.8.8";
    public const int PORT = 65530;

    public static IPAddress GetLocalIPAddress(string host = HOST, int port = PORT)
    {
        IPAddress localIP;
        using (Socket socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, 0))
        {
            socket.Connect(host, port);
            IPEndPoint endPoint = socket.LocalEndPoint as IPEndPoint;
            localIP = endPoint.Address;
        }
        return localIP;
    }
}
