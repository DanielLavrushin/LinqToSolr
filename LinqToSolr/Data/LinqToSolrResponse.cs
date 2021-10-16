using System;

using LinqToSolr.Helpers.Json;

namespace LinqToSolr.Data
{
    public class LinqToSolrResponse<T> : ILinqToSolrResponse<T>
    {
        public int FoundDocuments { get; set; }
        public Uri LastServiceUri { get; set; }
        public string Content { get; set; }

        [JsonProperty("responseHeader")]
        public LinqToSolrResponseHeader Header { get; set; }

        [JsonProperty("response")]
        public LinqToSolrResponseBody<T> Body { get; set; }

        [JsonProperty("error")]
        public LinqToSolrResponseError Error { get; set; }

        [JsonProperty("facet_counts")]
        public LinqToSolrResponseFacets Facets { get; set; }
    }
}
