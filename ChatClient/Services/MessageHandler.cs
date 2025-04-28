using ChatClient.Interfaces;

namespace ChatClient.Services
{
    public class MessageHandler : IMessageHandler
    {
        public Task HandleMessageAsync(string message)
        {
            DisplayMessage(message);
            return Task.CompletedTask;
        }

        public void DisplayMessage(string message)
        {
            Console.WriteLine(message);
        }
    }
} 