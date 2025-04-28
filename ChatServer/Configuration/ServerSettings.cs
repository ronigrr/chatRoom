namespace ChatServer.Configuration
{
    public class ServerSettings
    {
        public int Port { get; set; } = 5000;
        public string Host { get; set; } = "0.0.0.0";
        public int BufferSize { get; set; } = 1024;
    }
} 