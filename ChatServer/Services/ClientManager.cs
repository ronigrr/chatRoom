using System.Net.Sockets;
using ChatServer.Interfaces;

namespace ChatServer.Services
{
    public class ClientManager : IClientManager
    {
        private readonly Dictionary<string, TcpClient> _clients;
        private readonly object _lockObject;

        public ClientManager()
        {
            _clients = new Dictionary<string, TcpClient>();
            _lockObject = new object();
        }

        public void AddClient(string name, TcpClient client)
        {
            lock (_lockObject)
            {
                _clients[name] = client;
            }
        }

        public void RemoveClient(string name)
        {
            lock (_lockObject)
            {
                if (_clients.TryGetValue(name, out var client))
                {
                    client.Close();
                    _clients.Remove(name);
                }
            }
        }

        public TcpClient GetClient(string name)
        {
            lock (_lockObject)
            {
                return _clients.GetValueOrDefault(name) ?? throw new KeyNotFoundException($"Client {name} not found");
            }
        }

        public IEnumerable<string> GetAllClientNames()
        {
            lock (_lockObject)
            {
                return _clients.Keys.ToList();
            }
        }

        public bool IsClientConnected(string name)
        {
            lock (_lockObject)
            {
                return _clients.TryGetValue(name, out var client) && client.Connected;
            }
        }
    }
}