using Newtonsoft.Json;

namespace LinqToSolr.Data
{
    public class LinqToSolrResponseHeader
    {
        [JsonProperty("status")]
        public int Status { get; set; }

        [JsonProperty("QTime")]
        public int Time { get; set; }
    }
}
