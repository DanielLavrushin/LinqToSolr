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
    public enum LinqToSolrHttpMethod
    {
        GET,
        POST,
        PUT,
        DELETE
    }

    public interface ILinqToSolrResponse<TObject>
    {
        TObject Results { get; }
    }

    public class LinqToSolrResponse<TObject> : ILinqToSolrResponse<TObject>
    {
        public TObject Results { get; }
    }

    public interface ILinqToSolrRequest<TObject>
    {
        NameValueCollection QueryParameters { get; }
        LinqToSolrHttpMethod Method { get; }
        Uri GetCoreUri();
    }

    public class LinqToSolrRequest<TObject> : ILinqToSolrRequest<TObject>
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
        Task<ILinqToSolrResponse<TObject>> Execute<TObject>(ILinqToSolrRequest<TObject> request);
    }

    public class LinqToSolrHttpClient : ILinqToSolrHttpClient
    {
        public ILinqToSolrProvider Provider { get; }

        public LinqToSolrHttpClient(ILinqToSolrProvider provider)
        {
            Provider = provider;
        }
        public async Task<ILinqToSolrResponse<TObject>> Execute<TObject>(ILinqToSolrRequest<TObject> request)
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

                var response = new LinqToSolrResponse<TObject>();
                var httpResponse = await client.SendAsync(httpRequestMessage);
                var responseContent = await httpResponse.Content.ReadAsStringAsync();

                Debug.WriteLine(uriBuilder.Uri);
                Debug.WriteLine(responseContent);
                return response;
            }
        }

        public void Dispose()
        {
        }


    }
}
