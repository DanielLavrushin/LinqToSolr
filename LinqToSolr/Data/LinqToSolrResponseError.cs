using LinqToSolr.Helpers.Json;


namespace LinqToSolr.Data
{
    public class LinqToSolrResponseError
    {
        [JsonProperty("msg")]
        public string Message { get; set; }

        [JsonProperty("code")]
        public int Code { get; set; }
    }
}
