using ChatServer.Configuration;
using ChatServer.Interfaces;
using ChatServer.Services;

namespace ChatServer
{
    class Program
    {
        private static async Task Main(string[] args)
        {
            var settings = new ServerSettings();
            var clientManager = new ClientManager();
            var messageProcessor = new MessageProcessor();
            var chatServer = new ChatServerService(clientManager, messageProcessor, settings);

            try
            {
                await chatServer.StartAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Server error: {ex.Message}");
            }
        }
    }
}
