using System.Collections.Generic;

namespace elasticsearch.Models.Tasks
{
    public class IndexingTask
    {
        public string NameIndex { get; set; }
        public string RequestObj { get; set; }
        public string ResponseContent { get; set; }
        public int StatusCode { get; set; }
        public double ElapsedTime { get; set; }
        public string RequestMethod { get; set; }
        public string RequestQuery { get; set; }
        public string RequestPath { get; set; }
        public Dictionary<string, string[]> RequestHeader { get; set; }
        public Dictionary<string, string[]> ResponseHeader { get; set; }
    }
}