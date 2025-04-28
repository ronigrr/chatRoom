using System.Net.Sockets;
using System.Text;
using ChatClient.Configuration;
using ChatClient.Interfaces;

namespace ChatClient.Services
{
    public class ChatClientService : IChatClient
    {
        private readonly TcpClient _client;
        private NetworkStream _stream;
        private readonly IMessageHandler _messageHandler;
        private readonly ClientSettings _settings;
        private readonly string _serverAddress;
        private readonly int _port;
        private readonly string _userName;
        public bool IsConnected { get; private set; }

        public ChatClientService(IMessageHandler messageHandler, ClientSettings settings, string userName)
        {
            _messageHandler = messageHandler;
            _settings = settings;
            _serverAddress = settings.ServerAddress;
            _port = settings.Port;
            _userName = userName;
            _client = new TcpClient();
            IsConnected = false;
        }

        public async Task ConnectAsync()
        {
            try
            {
                await _client.ConnectAsync(_serverAddress, _port);
                _stream = _client.GetStream();

                var nameBuffer = Encoding.UTF8.GetBytes(_userName);
                await _stream.WriteAsync(nameBuffer);

                IsConnected = true;
                
                _messageHandler.DisplayMessage("Connected successfully!");
                
                _ = ReceiveMessagesAsync();
            }
            catch (Exception ex)
            {
                _messageHandler.DisplayMessage($"Failed to connect to server: {ex.Message}");
                throw;
            }
        }

        public Task DisconnectAsync()
        {
            IsConnected = false;
            
            try
            {
                _stream.Close();
                _client.Close();
            }
            catch (Exception ex)
            {
                _messageHandler.DisplayMessage($"Error during disconnect: {ex.Message}");
            }

            return Task.CompletedTask;
        }

        public async Task SendMessageAsync(string message)
        {
            if (!IsConnected)
            {
                throw new InvalidOperationException("Client is not connected to the server.");
            }

            try
            {
                var buffer = Encoding.UTF8.GetBytes(message);
                await _stream.WriteAsync(buffer);
            }
            catch (Exception ex)
            {
                _messageHandler.DisplayMessage($"Error sending message: {ex.Message}");
                await DisconnectAsync();
                throw;
            }
        }

        private async Task ReceiveMessagesAsync()
        {
            try
            {
                var buffer = new byte[_settings.BufferSize];
                while (IsConnected)
                {
                    var bytesRead = await _stream.ReadAsync(buffer);
                    if (bytesRead == 0)
                    {
                        await DisconnectAsync();
                        break;
                    }

                    var message = Encoding.UTF8.GetString(buffer, 0, bytesRead);
                    await _messageHandler.HandleMessageAsync(message);
                }
            }
            catch (Exception ex)
            {
                _messageHandler.DisplayMessage($"Error receiving messages: {ex.Message}");
                await DisconnectAsync();
            }
        }
    }
} 