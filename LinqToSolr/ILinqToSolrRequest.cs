using LinqToSolr.Expressions;
using System;
using System.Collections.Specialized;

#if !NETSTANDARD1_1 && !NETSTANDARD1_3 && !NETSTANDARD1_6
#endif

namespace LinqToSolr
{
    public interface ILinqToSolrRequest
    {
        ITranslatedQuery Translated { get; }
        NameValueCollection QueryParameters { get; }
        string Body { get; set; }
        string ContentType { get; }
        Uri GetCoreUri();
        Uri GetFinalUri();
    }

}
