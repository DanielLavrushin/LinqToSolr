using System;
using Newtonsoft.Json;

namespace LinqToSolr.Data
{
    public class LinqToSolrResponse: ILinqToSolrResponse
    {
        public int FoundDocuments { get; set; }
        public Uri LastServiceUri { get; set; }

        [JsonProperty("responseHeader")]
        public LinqToSolrResponseHeader Header { get; set; }

        [JsonProperty("response")]
        public LinqToSolrResponseBody Body { get; set; }

        [JsonProperty("error")]
        public LinqToSolrResponseError Error { get; set; }

        [JsonProperty("facet_counts")]
        public LinqToSolrResponseFacets Facets { get; set; }
    }
}
