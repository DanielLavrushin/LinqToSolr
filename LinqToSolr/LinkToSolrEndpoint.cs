using System;

namespace LinqToSolr
{
    public class LinkToSolrEndpoint : ILinkToSolrEndpoint
    {
        public Uri SolrUri { get; set; }

        public LinkToSolrEndpoint(string solrUrl)
        {
            SolrUri = new Uri(solrUrl);
            if (string.IsNullOrEmpty(SolrUri.AbsolutePath))
            {
                SolrUri = new Uri(SolrUri, "solr");
            }
        }
    }
}