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

    internal class LinqToSolrRequest
    {
        public TranslatedQuery Translated { get; }
        public NameValueCollection QueryParameters { get; private set; }
        public HttpMethod Method { get; }
        ILinqToSolrProvider _provider;
        public LinqToSolrRequest(ILinqToSolrProvider provider, TranslatedQuery expressionQuery, HttpMethod method)
        {
            Translated = expressionQuery;
            Method = method;
            _provider = provider;

            QueryParameters = HttpUtility.ParseQueryString(string.Empty);
            foreach (var filter in expressionQuery.Filters)
            {
                QueryParameters.Add("fq", filter);
            }
            QueryParameters["q"] = "*";
            QueryParameters["wt"] = "json";
            QueryParameters["start"] = expressionQuery.Skip.ToString();
            QueryParameters["rows"] = expressionQuery.Take.ToString();
            QueryParameters["indent"] = false.ToString().ToLower();

            if (expressionQuery.Sorting.Count > 0)
            {
                QueryParameters["sort"] = string.Join(",", expressionQuery.Sorting.Reverse().Select(x => $"{x.Key} {x.Value}").ToArray());
            }
            if (expressionQuery.Select.Count > 0)
            {
                QueryParameters["fl"] = string.Join(",", expressionQuery.Select.Select(x => x.Key).ToArray());
            }
        }

        public Uri GetCoreUri()
        {
            var solrCore = _provider.Service.Configuration.GetCore(_provider.ElementType);
            var solrUri = new Uri(_provider.Service.Configuration.Endpoint.SolrUri, $"{solrCore}/select");
            return solrUri;
        }
    }

}
