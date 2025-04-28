namespace ChatClient.Interfaces
{
    public interface IMessageHandler
    {
        Task HandleMessageAsync(string message);
        void DisplayMessage(string message);
    }
} 