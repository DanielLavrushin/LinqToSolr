using LinqToSolr.Expressions;
using LinqToSolr.Extensions;
using LinqToSolr.Providers;
using System;
using System.Net;
using System.Collections.Specialized;
using System.Linq;
using System.Net.Http;
#if !NETSTANDARD1_1 &&  !NETSTANDARD1_3 && !NETSTANDARD1_6
using System.Web;
#endif

namespace LinqToSolr
{
    internal class LinqToSolrRequest
    {
        public ITranslatedQuery Translated { get; }
        public NameValueCollection QueryParameters { get; private set; }
        public string Body { get; set; }
        public string ContentType
        {
            get
            {
                var contentType = "application/json";

                if (Translated.Method == HttpMethod.Post)
                {
                    contentType = "application/x-www-form-urlencoded";
                }

                return contentType;
            }
        }
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
            if (expressionQuery.Method == HttpMethod.Post)
            {
                Body = this.ParseQueryString();
            }
        }

        public Uri GetCoreUri()
        {
            var endpointMethod = "/select";
            if (Translated.Method == HttpMethod.Put || Translated.Method == HttpMethod.Delete)
            {
                endpointMethod = "/update";
            }
            var solrCore = _provider.Service.Configuration.GetCore(_provider.ElementType);
            var solrUri = new Uri(_provider.Service.Configuration.Endpoint.SolrUri, $"{solrCore}{endpointMethod}");
            return solrUri;
        }
        public Uri GetFinalUri()
        {
            var uri = GetCoreUri();
            var uriBuilder = new UriBuilder(uri);
            uriBuilder.Query = this.ParseQueryString();
            return Translated.Method == HttpMethod.Post ? uri : uriBuilder.Uri;
        }
        internal static LinqToSolrRequest InitUpdate<TSource>(ILinqToSolrProvider provider, TSource[] documents)
        {
            var translated = new TranslatedQuery() { Method = HttpMethod.Put };
            var request = new LinqToSolrRequest(provider, translated);
            request.QueryParameters = HttpUtility.ParseQueryString(string.Empty);
            request.QueryParameters.Add("commit", "true");
            request.QueryParameters.Add("softCommit", "true");
            request.QueryParameters.Add("wt", "json");
            request.QueryParameters.Add("versions", "true");
            request.QueryParameters.Add("overwrite", "true");
            if (documents != null && documents.Any())
            {
                request.Body = JsonWriter.ToJson(documents);
            }
            else
            {
                request.Body = null;
            }
            return request;
        }
    }

}
