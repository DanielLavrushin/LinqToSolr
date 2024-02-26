using LinqToSolr.Expressions;
using LinqToSolr.Extensions;
using LinqToSolr.Providers;
using System;
using System.Net;
using System.Collections.Specialized;
using System.Linq;
#if !NETSTANDARD1_0
using System.Net.Http;
#endif
#if NETSTANDARD2_0_OR_GREATER
using System.Web;
#endif

namespace LinqToSolr
{
    internal class LinqToSolrRequest
    {
        public ITranslatedQuery Translated { get; }
        public NameValueCollection QueryParameters { get; private set; }
        ILinqToSolrProvider _provider;
        public LinqToSolrRequest(ILinqToSolrProvider provider, ITranslatedQuery expressionQuery)
        {
            Translated = expressionQuery;
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
            if (expressionQuery.Groups.Any())
            {
                QueryParameters.Add("group.limit", expressionQuery.Take.ToString());
                QueryParameters.Add("group.offset", expressionQuery.Skip.ToString());
                QueryParameters.Add("group", "true");
                foreach (var group in expressionQuery.Groups)
                {
                    QueryParameters.Add("group.field", group);
                }
            }

            if (expressionQuery.Facets.Count > 0)
            {
                QueryParameters.Add("facet", "true");
                QueryParameters.Add("facet.limit", expressionQuery.Take.ToString());
                QueryParameters.Add("facet.mincount", "1");
                QueryParameters["rows"] = "0";
                foreach (var facet in expressionQuery.Facets)
                {
                    QueryParameters.Add("facet.field", facet.Key);
                }
            }

            foreach (var filter in expressionQuery.Filters)
            {
                QueryParameters.Add("fq", filter);
            }

            if (expressionQuery.Sorting.Count > 0)
            {
                QueryParameters["sort"] = string.Join(",", expressionQuery.Sorting.Reverse().Select(x => $"{x.Key} {x.Value}").ToArray());
            }

            if (expressionQuery.Select.Count > 0)
            {
                QueryParameters["fl"] = string.Join(",", expressionQuery.Select.Select(x => x.Key).ToArray());
            }

            QueryParameters["indent"] = false.ToString().ToLower();
        }

        public Uri GetCoreUri()
        {
            var solrCore = _provider.Service.Configuration.GetCore(_provider.ElementType);
            var solrUri = new Uri(_provider.Service.Configuration.Endpoint.SolrUri, $"{solrCore}/select");
            return solrUri;
        }
    }

}
