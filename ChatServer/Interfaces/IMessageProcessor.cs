namespace ChatServer.Interfaces
{
    public interface IMessageProcessor
    {
        Task ProcessMessageAsync(string senderName, string message);
        void UpdateLetterCount(string message);
        Dictionary<char, int> GetLetterCount();
    }
} 