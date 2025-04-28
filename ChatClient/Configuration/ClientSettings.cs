namespace ChatClient.Configuration
{
    public class ClientSettings
    {
        public string ServerAddress { get; set; } = "localhost";
        public int Port { get; set; } = 5000;
        public int BufferSize { get; set; } = 1024;
    }
} 