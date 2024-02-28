using LinqToSolr.Expressions;
using LinqToSolr.Extensions;
using LinqToSolr.Providers;
using System;
using System.Net;
using System.Collections.Specialized;
using System.Linq;
using System.Net.Http;
using System.Linq.Expressions;

#if !NETSTANDARD1_1 && !NETSTANDARD1_3 && !NETSTANDARD1_6
using System.Web;
#endif

namespace LinqToSolr
{
    internal class LinqToSolrRequest : ILinqToSolrRequest
    {
        public ITranslatedQuery Translated { get; private set; }
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

            foreach (var filter in Translated.Filters)
            {
                QueryParameters.Add("fq", filter);
            }
            QueryParameters["q"] = "*";
            QueryParameters["wt"] = "json";
            QueryParameters["start"] = Translated.Skip.ToString();
            QueryParameters["rows"] = Translated.Take.ToString();
            if (Translated.Groups.Any())
            {
                QueryParameters.Add("group.limit", Translated.Take.ToString());
                QueryParameters.Add("group.offset", Translated.Skip.ToString());
                QueryParameters.Add("group", "true");
                foreach (var group in Translated.Groups)
                {
                    QueryParameters.Add("group.field", group);
                }
            }

            if (Translated.Facets.Count > 0)
            {
                QueryParameters.Add("facet", "true");
                QueryParameters.Add("facet.limit", Translated.Take.ToString());
                QueryParameters.Add("facet.mincount", "1");
                QueryParameters["rows"] = "0";
                foreach (var facet in Translated.Facets)
                {
                    QueryParameters.Add("facet.field", facet.Key);
                }
            }

            foreach (var filter in Translated.Filters)
            {
                QueryParameters.Add("fq", filter);
            }

            if (Translated.Sorting.Count > 0)
            {
                QueryParameters["sort"] = string.Join(",", Translated.Sorting.Reverse().Select(x => $"{x.Key} {x.Value}").ToArray());
            }

            if (Translated.Select.Count > 0)
            {
                QueryParameters["fl"] = string.Join(",", Translated.Select.Select(x => x.Key).ToArray());
            }

            QueryParameters["indent"] = false.ToString().ToLower();
            if (Translated.Method == HttpMethod.Post)
            {
                Body = this.ParseQueryString();
            }

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

        internal static LinqToSolrRequest InitDelete<TSource>(LinqToSolrProvider provider, ITranslatedQuery query)
        {
            var translated = new TranslatedQuery() { Method = HttpMethod.Delete };

            var request = new LinqToSolrRequest(provider, translated);
            request.QueryParameters = HttpUtility.ParseQueryString(string.Empty);
            request.QueryParameters.Add("wt", "json");
            request.QueryParameters.Add("commit", "true");
            request.QueryParameters.Add("softCommit", "true");
            request.Body = new { delete = new { query = string.Join(" AND ", query.Filters.ToArray()) } }.ToJson();
            return request;

        }
    }

}
