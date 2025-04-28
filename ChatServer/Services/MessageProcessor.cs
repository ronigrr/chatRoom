using ChatServer.Interfaces;

namespace ChatServer.Services
{
    public class MessageProcessor : IMessageProcessor
    {
        private IChatServer? _chatServer;
        private readonly Dictionary<char, int> _totalLetterCount;
        private readonly object _lockObject;
        private const string PrivateMessagePrefix = "To: ";
        private const string PrivateChatSeparator = " - ";

        public MessageProcessor()
        {
            _totalLetterCount = new Dictionary<char, int>();
            _lockObject = new object();
        }

        public void SetServer(IChatServer chatServer)
        {
            _chatServer = chatServer;
        }

        public async Task ProcessMessageAsync(string senderName, string message)
        {
            if (_chatServer == null)
            {
                throw new InvalidOperationException("Server is not set");
            }

            var messageToCount = message;
            if (IsPrivateMessage(message, out var targetName, out var content))
            {
                if (targetName == null || content == null)
                {
                    throw new InvalidOperationException("Invalid private message format");
                }

                await _chatServer.SendPrivateMessageAsync(senderName, targetName, content);
                messageToCount = content;
            }
            else
            {
                await _chatServer.BroadcastMessageAsync(
                    $"{DateTime.Now:dd/MM/yyyy HH:mm:ss}: {senderName} - {message}");
            }

            UpdateLetterCount(messageToCount);
            PrintLetterHistory();
        }

        public void UpdateLetterCount(string message)
        {
            lock (_lockObject)
            {
                foreach (var c in message.ToLower())
                {
                    if (char.IsLetter(c))
                    {
                        if (!_totalLetterCount.TryAdd(c, 1))
                            _totalLetterCount[c]++;
                    }
                }
            }
        }

        public Dictionary<char, int> GetLetterCount()
        {
            lock (_lockObject)
            {
                return new Dictionary<char, int>(_totalLetterCount);
            }
        }

        private static bool IsPrivateMessage(string message, out string? targetName, out string? content)
        {
            targetName = null;
            content = null;

            if (!message.StartsWith(PrivateMessagePrefix))
            {
                return false;
            }

            var parts = message.Substring(PrivateMessagePrefix.Length).Split(PrivateChatSeparator, 2);
            if (parts.Length != 2)
            {
                return false;
            }

            targetName = parts.First().Trim();
            content = parts.Last();
            return true;
        }

        private void PrintLetterHistory()
        {
            lock (_lockObject)
            {
                var frequentLetters = _totalLetterCount
                    .Where(kvp => kvp.Value >= 2)
                    .OrderByDescending(kvp => kvp.Value)
                    .ThenBy(kvp => kvp.Key)
                    .ToList();

                if (frequentLetters.Count == 0)
                {
                    return;
                }

                Console.WriteLine("\nLetter History:");
                Console.WriteLine(string.Join("\n",
                    frequentLetters.Select(kvp => $"Letter '{kvp.Key}': {kvp.Value} times")));
                Console.WriteLine();
            }
        }
    }
}