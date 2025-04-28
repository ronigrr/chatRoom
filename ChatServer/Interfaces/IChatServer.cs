namespace ChatServer.Interfaces
{
    public interface IChatServer
    {
        Task StartAsync();
        Task StopAsync();
        Task BroadcastMessageAsync(string message);
        Task SendPrivateMessageAsync(string senderName, string targetName, string message);
        void RemoveClient(string clientName);
    }
} 