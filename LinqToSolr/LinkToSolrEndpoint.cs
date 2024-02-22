using System;
using System.Diagnostics;
using System.Linq;

namespace LinqToSolr
{
    public class LinkToSolrEndpoint : ILinkToSolrEndpoint
    {
        public Uri SolrUri { get; set; }

        public LinkToSolrEndpoint(string solrUrl)
        {
            Uri uri = new Uri(solrUrl);
            string basePath = "/" +  (uri.AbsolutePath.Split(new[] { '/' }, StringSplitOptions.RemoveEmptyEntries).FirstOrDefault() ?? "solr");
            Uri baseUri = new Uri(uri, basePath);

            string normalizedUri = baseUri.GetLeftPart(UriPartial.Authority) + basePath + "/";
            SolrUri = new Uri(normalizedUri);
            Debug.WriteLine("SolrUri: " + SolrUri);
        }
    }
}