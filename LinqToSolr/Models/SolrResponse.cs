using System;
using System.Collections.Generic;

using LinqToSolr.Helpers.Json;
using LinqToSolr.Interfaces;
using LinqToSolr.Models;

namespace LinqToSolr.Models
{

    public class SolrResponse
    {
        [SolrField("responseHeader")]
        public ResponseHeader Header { get; set; }
        [SolrField("error")]
        public ResponseError Error { get; set; }
    }

    public class SolrResponse<TResult> : SolrResponse
    {
        [SolrField("response")]
        public ResponseBody<TResult> Response { get; set; }

        [SolrField("facet_counts")]
        public ResponseFacets<TResult> Facets { get; set; }

        [SolrField("grouped")]
        public Dictionary<string, ResponseGroupField<TResult>> Groups { get; set; }
    }
}
