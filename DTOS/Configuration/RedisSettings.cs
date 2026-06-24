namespace Minimart_Api.DTOS.Configuration
{
    public class RedisSettings
    {
        public bool UseSSL { get; set; } = false;
        public int ConnectTimeout { get; set; } = 10000;
        public int SyncTimeout { get; set; } = 5000;
        public bool AbortOnConnectFail { get; set; } = false;
        public int Database { get; set; } = 0;
        public bool IsUpstash { get; set; } = false;
        public string? UpstashEndpoint { get; set; }    
    }
}