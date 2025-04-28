using ChatClient.Configuration;
using ChatClient.Services;

namespace ChatClient
{
    class Program
    {
        static async Task Main(string[] args)
        {
            var userName = "";
            while (string.IsNullOrWhiteSpace(userName))
            {
                Console.WriteLine("Please enter Name:");
                userName = Console.ReadLine();
                if (string.IsNullOrEmpty(userName))
                {
                    Console.WriteLine("You must enter a name.");
                }
            }

            var settings = new ClientSettings();
            var messageHandler = new MessageHandler();
            var chatClient = new ChatClientService(messageHandler, settings, userName);

            try
            {
                await chatClient.ConnectAsync();

                while (chatClient.IsConnected)
                {
                    var message = Console.ReadLine();
                    if (string.IsNullOrEmpty(message)) continue;

                    await chatClient.SendMessageAsync(message);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
            }
            finally
            {
                await chatClient.DisconnectAsync();
            }
        }
    }
}
