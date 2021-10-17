using LinqToSolr.Helpers.Json;

namespace LinqToSolr.Data
{
    public class LinqToSolrResponseHeader
    {
        [SolrField("status")]
        public int Status { get; set; }

        [SolrField("QTime")]
        public int Time { get; set; }
    }
}
