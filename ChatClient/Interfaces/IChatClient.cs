namespace ChatClient.Interfaces
{
    public interface IChatClient
    {
        Task ConnectAsync();
        Task DisconnectAsync();
        Task SendMessageAsync(string message);
        bool IsConnected { get; }
    }
} 