using LinqToSolr.Attributes;
using LinqToSolr.Extensions;
using LinqToSolr.Providers;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Net;

namespace LinqToSolr
{
    internal class LinqToSolrResponseHeader
    {
        public HttpStatusCode Status { get; set; }
        public int QTime { get; set; }
        public IDictionary<string, string> Params { get; set; }
    }
    internal class LinqToSolrResponseBase
    {
        [LinqToSolrField("responseHeader")]
        public LinqToSolrResponseHeader Header { get; set; }
    }
    public interface ILinqToSolrFinalResponse<TObject>
    {
        TObject GetDocuments();

    }
    internal class LinqToSolrResponse<TObject> : LinqToSolrResponseBase, ILinqToSolrFinalResponse<TObject>
    {
        [LinqToSolrField("response")]
        public LinqToSolrResponseBody<TObject> Response { get; set; } = new LinqToSolrResponseBody<TObject>();

        public TObject GetDocuments()
        {
            return Response.Result;
        }
    }

    internal class LinqToSolrResponseBody<TObject>
    {
        public int NumFound { get; set; }
        public int Start { get; set; }
        [LinqToSolrField("docs")]
        public TObject Result { get; set; }
    }

    internal class LinqToSolrGroupResponse<TObject> : LinqToSolrResponseBase, ILinqToSolrFinalResponse<TObject>
    {
        [LinqToSolrField("grouped")]
        public IDictionary<string, LinqToSolrResponseGroupField<TObject>> Grouped { get; set; }

        public TObject GetDocuments()
        {
            return Grouped.FirstOrDefault().Value.Groups.FirstOrDefault().Result.Documents;
        }
    }
    internal class LinqToSolrResponseGroupField<TObject>
    {
        [LinqToSolrField("matches")]
        public int Matches { get; set; }

        [LinqToSolrField("groups")]
        public IEnumerable<LinqToSolrResponseGroupBody<TObject>> Groups { get; set; }
    }
    internal class LinqToSolrResponseGroupBody<TObject>
    {
        [LinqToSolrField("groupValue")]
        public object Id { get; set; }

        [LinqToSolrField("doclist")]
        public LinqToSolrResponseFieldGroup<TObject> Result { get; set; }
    }

    internal class LinqToSolrResponseError : LinqToSolrResponseBase
    {
        public LinqToSolrError Error { get; set; }
    }

    internal class LinqToSolrResponseFieldGroup<TObject>
    {
        [LinqToSolrField("numFound")]
        public int NumFound { get; set; }
        [LinqToSolrField("start")]
        public int Start { get; set; }
        [LinqToSolrField("docs")]
        public TObject Documents { get; set; }
        [LinqToSolrField("numFoundExact")]
        public bool NumFoundExact { get; set; }
    }

    internal class LinqToSolrError
    {
        public string[] Metadata { get; set; }

        [LinqToSolrField("Msg")]
        public string Mesasge { get; set; }
        public HttpStatusCode Code { get; set; }
    }
}
