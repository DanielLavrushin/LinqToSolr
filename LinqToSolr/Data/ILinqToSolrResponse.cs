using System;

namespace LinqToSolr.Data
{
    public interface ILinqToSolrResponse<T>
    {
        int FoundDocuments { get; set; }
        string Content { get; set; }
        Uri LastServiceUri { get; set; }
        LinqToSolrResponseHeader Header { get; set; }
        LinqToSolrResponseBody<T> Body { get; set; }
        LinqToSolrResponseFacets Facets { get; set; }
        LinqToSolrResponseError Error { get; set; }
    }
}
