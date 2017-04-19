using System;

namespace LinqToSolr.Data
{
    public interface ILinqToSolrResponse
    {
        int FoundDocuments { get; set; }
        Uri LastServiceUri { get; set; }
        LinqToSolrResponseHeader Header { get; set; }
        LinqToSolrResponseBody Body { get; set; }
        LinqToSolrResponseFacets Facets { get; set; }
        LinqToSolrResponseError Error { get; set; }
    }
}
