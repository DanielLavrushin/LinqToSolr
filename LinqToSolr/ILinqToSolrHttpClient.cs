using LinqToSolr.Extensions;
using LinqToSolr.Providers;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace LinqToSolr
{

    public class LinqToSolrRequest<TObject>
    {
        public NameValueCollection QueryParameters { get; private set; }
        public LinqToSolrHttpMethod Method { get; }
        ILinqToSolrHttpClient _client;
        public LinqToSolrRequest(ILinqToSolrHttpClient client, string expressionQuery, LinqToSolrHttpMethod method)
        {
            Method = method;
            _client = client;
            QueryParameters = HttpUtility.ParseQueryString(string.Empty);
            QueryParameters["q"] = "*";
            QueryParameters["wt"] = "json";
            QueryParameters["fq"] = expressionQuery;
            QueryParameters["indent"] = false.ToString().ToLower();
        }

        public Uri GetCoreUri()
        {
            var solrCore = _client.Provider.Service.Configuration.GetCore(_client.Provider.ElementType);
            var solrUri = new Uri(_client.Provider.Service.Configuration.Endpoint.SolrUri, $"{solrCore}/select");
            return solrUri;
        }
    }

    public interface ILinqToSolrHttpClient : IDisposable
    {
        ILinqToSolrProvider Provider { get; }
        Task<LinqToSolrResponse<TObject>> Execute<TObject>(LinqToSolrRequest<TObject> request);
    }

    public class LinqToSolrHttpClient : ILinqToSolrHttpClient
    {
        public ILinqToSolrProvider Provider { get; }

        public LinqToSolrHttpClient(ILinqToSolrProvider provider)
        {
            Provider = provider;
        }
        public async Task<LinqToSolrResponse<TObject>> Execute<TObject>(LinqToSolrRequest<TObject> request)
        {

            using (var client = new HttpClient())
            {
                var contentType = "application/json";
                var url = request.GetCoreUri();
                var uriBuilder = new UriBuilder(url);
                uriBuilder.Query = request.QueryParameters.ToString();

                var httpRequestMessage = new HttpRequestMessage(request.Method == LinqToSolrHttpMethod.GET ? HttpMethod.Get : HttpMethod.Post, uriBuilder.Uri)
                {
                    Content = null
                };

                var httpResponse = await client.SendAsync(httpRequestMessage);
                var responseContent = await httpResponse.Content.ReadAsStringAsync();

                if (httpResponse.StatusCode != System.Net.HttpStatusCode.OK)
                {
                    throw LinqToSolrException.ParseSolrErrorResponse(responseContent);
                }

                var response = JsonParser.FromJson<LinqToSolrResponse<TObject>>(responseContent);
                response.Header.Status = System.Net.HttpStatusCode.OK;

                Debug.WriteLine(uriBuilder.Uri);
                return response;
            }
        }

        public void Dispose()
        {
        }
        public class LinqToSolrException : Exception
        {
            public LinqToSolrResponseError ResponseError { get; private set; }

            private LinqToSolrException(string message, LinqToSolrResponseError responseError)
                : base(message)
            {
                ResponseError = responseError;
            }

            public static LinqToSolrException ParseSolrErrorResponse(string solrResponse)
            {
                var responseError = JsonParser.FromJson<LinqToSolrResponseError>(solrResponse);
                var message = responseError?.Error?.Mesasge ?? "An error occurred with the Solr response, but no specific message was provided.";
                return new LinqToSolrException(message, responseError);
            }
        }
    }
}
