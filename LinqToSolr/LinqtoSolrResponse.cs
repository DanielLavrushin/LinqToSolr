using LinqToSolr.Attributes;
using LinqToSolr.Extensions;
using LinqToSolr.Providers;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
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
    public interface ILinqToSolrUriResponse
    {
        Uri RequestUrl { get; set; }
    }
    public interface ILinqToSolrFinalResponse<TObject> : ILinqToSolrUriResponse
    {
        TObject GetDocuments();
        void SetDcouments(object documents);

    }
    public abstract class LinqToSolrFinalResponse<TObject> : LinqToSolrResponseBase, ILinqToSolrFinalResponse<TObject>
    {
        public bool Success => Header.Status == HttpStatusCode.OK;
        public Uri RequestUrl { get; set; }

        [EditorBrowsable(EditorBrowsableState.Never)]
        public virtual TObject GetDocuments()
        {
            throw new NotImplementedException();
        }

        [EditorBrowsable(EditorBrowsableState.Never)]
        public virtual void SetDcouments(object documents)
        {
            throw new NotImplementedException();
        }
    }
    public class LinqToSolrUpdateResponse<TObject> : LinqToSolrFinalResponse<TObject>
    {

    }
    public class LinqToSolrDeleteResponse<TObject> : LinqToSolrFinalResponse<TObject>
    {

    }

    public class LinqToSolrResponse<TObject> : LinqToSolrFinalResponse<TObject>
    {
        [LinqToSolrField("response")]
        public LinqToSolrResponseBody<TObject> Response { get; set; } = new LinqToSolrResponseBody<TObject>();

        public override TObject GetDocuments()
        {
            return Response.Result;
        }

        public override void SetDcouments(object documents)
        {
            Response.Result = (TObject)documents;
        }
    }

    public class LinqToSolrResponseBody<TObject>
    {
        public int NumFound { get; set; }
        public int Start { get; set; }
        [LinqToSolrField("docs")]
        public TObject Result { get; set; }
    }

    public class LinqToSolrGroupResponse<TObject> : LinqToSolrFinalResponse<TObject>
    {
        [LinqToSolrField("grouped")]
        public IDictionary<string, LinqToSolrResponseGroupField<TObject>> Grouped { get; set; }

        public override TObject GetDocuments()
        {
            return Grouped.FirstOrDefault().Value.Groups.FirstOrDefault().Result.Documents;
        }
    }

    public class LinqToSolrFacetsResponse<TObject> : LinqToSolrFinalResponse<TObject>
    {
        [LinqToSolrField("facet_counts")]
        public LinqToSolrFacetFields Result { get; set; }
    }

    public class LinqToSolrFacetFields
    {
        [LinqToSolrField("facet_fields")]
        public IDictionary<string, object[]> FacetFields { get; set; }
    }

    public class LinqToSolrResponseGroupField<TObject>
    {
        [LinqToSolrField("matches")]
        public int Matches { get; set; }

        [LinqToSolrField("groups")]
        public IEnumerable<LinqToSolrResponseGroupBody<TObject>> Groups { get; set; }
    }
    public class LinqToSolrResponseGroupBody<TObject>
    {
        [LinqToSolrField("groupValue")]
        public object Id { get; set; }

        [LinqToSolrField("doclist")]
        public LinqToSolrResponseFieldGroup<TObject> Result { get; set; }
    }

    public class LinqToSolrResponseError : LinqToSolrResponseBase
    {
        public LinqToSolrError Error { get; set; }
    }

    public class LinqToSolrResponseFieldGroup<TObject>
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

    public class LinqToSolrError
    {
        public string[] Metadata { get; set; }

        [LinqToSolrField("Msg")]
        public string Mesasge { get; set; }
        public HttpStatusCode Code { get; set; }
    }
}
