using LinqToSolr.Attributes;
using LinqToSolr.Extensions;
using LinqToSolr.Providers;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Net;

namespace LinqToSolr
{
    public class LinqToSolrResponseHeader
    {
        public HttpStatusCode Status { get; set; }
        public int QTime { get; set; }
        public IDictionary<string, string> Params { get; set; }
    }
    public class LinqToSolrResponseBase
    {
        [LinqToSolrField("responseHeader")]
        public LinqToSolrResponseHeader Header { get; set; }
    }

    public class LinqToSolrResponse<TObject> : LinqToSolrResponseBase
    {
        [LinqToSolrField("response")]
        public LinqToSolrResponseBody<TObject> Response { get; set; } = new LinqToSolrResponseBody<TObject>();
    }

    public class LinqToSolrResponseBody<TObject>
    {
        public int NumFound { get; set; }
        public int Start { get; set; }
        [LinqToSolrField("docs")]
        public TObject Result { get; set; }
    }
    public class LinqToSolrResponseError : LinqToSolrResponseBase
    {
        public LinqToSolrError Error { get; set; }
    }

    public class LinqToSolrError
    {
        public string[] Metadata { get; set; }

        [LinqToSolrField("Msg")]
        public string Mesasge { get; set; }
        public HttpStatusCode Code { get; set; }
    }
}
