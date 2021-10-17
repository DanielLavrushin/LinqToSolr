using System;
using System.Collections.Generic;

using LinqToSolr.Helpers.Json;
using LinqToSolr.Interfaces;
using LinqToSolr.Models;

namespace LinqToSolr.Data
{

    internal class SolrResponse
    {
        internal Uri LastServiceUri { get; set; }

        [SolrField("responseHeader")]
        internal ResponseHeader Header { get; set; }

        [SolrField("facet_counts")]
        internal ResponseFacets Facets { get; set; }

        [SolrField("error")]
        internal ResponseError Error { get; set; }
    }

    internal class SolrResponse<T> : SolrResponse
    {
        [SolrField("response")]
        internal ResponseBody<T> Response { get; set; }


    }
}
