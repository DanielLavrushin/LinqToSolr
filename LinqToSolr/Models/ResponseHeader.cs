using System.Collections.Generic;

namespace LinqToSolr.Models
{
    public class ResponseHeader
    {
        [SolrField("responseHeader")]
        public int Status { get; set; }

        [SolrField("qtime")]
        public int Time { get; set; }

        [SolrField("params")]
        public Dictionary<string, string> Query { get; set; }
    }
}
