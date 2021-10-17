using LinqToSolr.Helpers.Json;


namespace LinqToSolr.Data
{
    public class LinqToSolrResponseError
    {
        [SolrField("msg")]
        public string Message { get; set; }

        [SolrField("code")]
        public int Code { get; set; }
    }
}
