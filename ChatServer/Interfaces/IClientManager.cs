using System.Net.Sockets;

namespace ChatServer.Interfaces
{
    public interface IClientManager
    {
        void AddClient(string name, TcpClient client);
        void RemoveClient(string name);
        TcpClient GetClient(string name);
        IEnumerable<string> GetAllClientNames();
        bool IsClientConnected(string name);
    }
} 