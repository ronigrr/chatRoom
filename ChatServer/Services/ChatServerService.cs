using System.Net;
using System.Net.Sockets;
using System.Text;
using ChatServer.Configuration;
using ChatServer.Interfaces;

namespace ChatServer.Services
{
    public class ChatServerService : IChatServer
    {
        private readonly TcpListener _server;
        private readonly IClientManager _clientManager;
        private readonly IMessageProcessor _messageProcessor;
        private readonly ServerSettings _settings;
        private bool _isRunning;
        private readonly Lock _lockObject = new();

        public ChatServerService(IClientManager clientManager, IMessageProcessor messageProcessor, ServerSettings settings)
        {
            _clientManager = clientManager;
            _messageProcessor = messageProcessor;
            _settings = settings;
            _server = new TcpListener(IPAddress.Parse(_settings.Host), _settings.Port);
            
            if (messageProcessor is MessageProcessor processor)
            {
                processor.SetServer(this);
            }
        }

        public async Task StartAsync()
        {
            try
            {
                _server.Start();
                _isRunning = true;
                Console.WriteLine($"Server started on {_settings.Host}:{_settings.Port}");

                while (_isRunning)
                {
                    var client = await _server.AcceptTcpClientAsync();
                    _ = HandleClientAsync(client);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Server error: {ex.Message}");
                throw;
            }
        }

        public Task StopAsync()
        {
            lock (_lockObject)
            {
                _isRunning = false;
            }
            
            _server.Stop();
            var clientNames = _clientManager.GetAllClientNames();
            foreach (var name in clientNames)
            {
                _clientManager.RemoveClient(name);
            }

            return Task.CompletedTask;
        }

        public async Task BroadcastMessageAsync(string message)
        {
            var clientNames = _clientManager.GetAllClientNames();
            foreach (var name in clientNames)
            {
                try
                {
                    var client = _clientManager.GetClient(name);
                    if (client != null)
                    {
                        var stream = client.GetStream();
                        var buffer = Encoding.UTF8.GetBytes(message);
                        await stream.WriteAsync(buffer, 0, buffer.Length);
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error broadcasting to {name}: {ex.Message}");
                    _clientManager.RemoveClient(name);
                }
            }
        }

        public async Task SendPrivateMessageAsync(string senderName, string targetName, string message)
        {
            var targetClient = _clientManager.GetClient(targetName);
            var senderClient = _clientManager.GetClient(senderName);

            if (targetClient != null && senderClient != null)
            {
                var privateMessage = $"[Private] {DateTime.Now:dd/MM/yyyy HH:mm:ss}, {senderName} - {message}";
                var buffer = Encoding.UTF8.GetBytes(privateMessage);
                
                try
                {
                    var targetStream = targetClient.GetStream();
                    var senderStream = senderClient.GetStream();
                    await targetStream.WriteAsync(buffer);
                    await senderStream.WriteAsync(buffer);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error sending private message: {ex.Message}");
                    if (targetClient != null) _clientManager.RemoveClient(targetName);
                    if (senderClient != null) _clientManager.RemoveClient(senderName);
                }
            }
            else
            {
                var errorMessage = $"User {targetName} not found.";
                var buffer = Encoding.UTF8.GetBytes(errorMessage);
                
                if (senderClient != null)
                {
                    try
                    {
                        var senderStream = senderClient.GetStream();
                        await senderStream.WriteAsync(buffer, 0, buffer.Length);
                    }
                    catch
                    {
                        _clientManager.RemoveClient(senderName);
                    }
                }
            }
        }

        public void RemoveClient(string clientName)
        {
            _clientManager.RemoveClient(clientName);
        }

        private async Task HandleClientAsync(TcpClient client)
        {
            var clientName = string.Empty;
            try
            {
                var stream = client.GetStream();
                var buffer = new byte[_settings.BufferSize];

                var bytesRead = await stream.ReadAsync(buffer);
                clientName = Encoding.UTF8.GetString(buffer, 0, bytesRead);

                _clientManager.AddClient(clientName, client);

                Console.WriteLine($"Client {clientName} connected.");
                await BroadcastMessageAsync($"System: {clientName} has joined the chat.");

                var isRunning = false;
                lock (_lockObject)
                {
                    isRunning = _isRunning;
                }
                
                while (isRunning)
                {
                    bytesRead = await stream.ReadAsync(buffer);
                    if (bytesRead == 0) break;

                    var message = Encoding.UTF8.GetString(buffer, 0, bytesRead);
                    await _messageProcessor.ProcessMessageAsync(clientName, message);
                    
                    lock (_lockObject)
                    {
                        isRunning = _isRunning;
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error handling client {clientName}: {ex.Message}");
            }
            finally
            {
                if (!string.IsNullOrEmpty(clientName))
                {
                    RemoveClient(clientName);
                    Console.WriteLine($"Client {clientName} disconnected.");
                    await BroadcastMessageAsync($"System: {clientName} has left the chat.");
                }
            }
        }
    }
} 