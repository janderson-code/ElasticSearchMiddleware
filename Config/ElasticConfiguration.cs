namespace elasticsearch.Config
{
    public class ElasticConfiguration
    {
        public string Uri { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }
        public string DefaultIndex { get; set; }
        public string Environment { get; set; }
    }
}