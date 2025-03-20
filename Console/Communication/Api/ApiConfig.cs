namespace Console.Communication.Api
{
    public class ApiConfig
    {
        public string BaseUrl { get; set; }
        public string ApiKey { get; set; }
        public int TimeoutSeconds { get; set; } = 30;
    }
}
