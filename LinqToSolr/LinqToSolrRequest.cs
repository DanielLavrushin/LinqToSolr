using LinqToSolr.Expressions;
using LinqToSolr.Extensions;
using LinqToSolr.Providers;
using System;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web;

namespace LinqToSolr
{

    internal class LinqToSolrRequest<TObject>
    {
        public NameValueCollection QueryParameters { get; private set; }
        public HttpMethod Method { get; }
        ILinqToSolrProvider _provider;
        public LinqToSolrRequest(ILinqToSolrProvider provider, TranslatedQuery expressionQuery, HttpMethod method)
        {
            Method = method;
            _provider = provider;
            QueryParameters = HttpUtility.ParseQueryString(string.Empty);
            QueryParameters["q"] = "*";
            QueryParameters["wt"] = "json";
            QueryParameters["start"] = expressionQuery.Skip.ToString();
            QueryParameters["rows"] = expressionQuery.Take.ToString();
            QueryParameters["fq"] = expressionQuery.Query;
            QueryParameters["indent"] = false.ToString().ToLower();

            if (expressionQuery.Sorting.Count > 0)
            {
                QueryParameters["sort"] = string.Join(",", expressionQuery.Sorting.Reverse().Select(x => string.Format("{0} {1}", x.Key, x.Value)).ToArray());
            }

            Debug.WriteLine($"Query: {expressionQuery.Query}");
        }

        public Uri GetCoreUri()
        {
            var solrCore = _provider.Service.Configuration.GetCore(_provider.ElementType);
            var solrUri = new Uri(_provider.Service.Configuration.Endpoint.SolrUri, $"{solrCore}/select");
            return solrUri;
        }
    }

}
