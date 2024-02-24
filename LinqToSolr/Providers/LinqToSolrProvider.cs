using LinqToSolr.Expressions;
using LinqToSolr.Extensions;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;
using System.Net.Http;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace LinqToSolr.Providers
{
    public class LinqToSolrProvider : ILinqToSolrProvider
    {
        public Type ElementType { get; }
        public ILinqToSolrService Service { get; }

        public LinqToSolrProvider(ILinqToSolrService service, Type elementType)
        {
            Service = service;
            ElementType = elementType;
        }

        public IQueryable CreateQuery(Expression expression)
        {
            var elementType = expression.Type.GetGenericArguments().First();
            var queryableType = typeof(LinqToSolrQueriable<>).MakeGenericType(elementType);
            return (IQueryable)Activator.CreateInstance(queryableType, new object[] { this, expression });
        }

        public IQueryable<TElement> CreateQuery<TElement>(Expression expression)
        {
            return new LinqToSolrQueriable<TElement>(this, expression);
        }

        public object Execute(Expression expression)
        {
            var executeMethod = typeof(LinqToSolrProvider).GetMethod(nameof(ExecuteAsync), BindingFlags.NonPublic | BindingFlags.Instance).MakeGenericMethod(ElementType);
            var task = (Task)executeMethod.Invoke(this, new[] { expression });
            return task.GetType().GetProperty("Result").GetValue(task);
        }

        public TResult Execute<TResult>(Expression expression)
        {
            return ExecuteAsync<TResult>(expression).GetAwaiter().GetResult();
        }

        public async Task<TResult> ExecuteAsync<TResult>(Expression expression)
        {
            var translator = new ExpressionTranslator<TResult>(expression);
            var query = translator.Translate(expression);
            var request = new LinqToSolrRequest(this, query, HttpMethod.Get);
            var response = await PrepareAndSendAsync<TResult>(request);
            var docs = response.Response.Result;
            return docs;
        }

        internal async Task<LinqToSolrResponse<TObject>> PrepareAndSendAsync<TObject>(LinqToSolrRequest request)
        {
            var returnType = typeof(TObject);
            if (request.Translated.IsSelect)
            {
                var isCollection = returnType.IsGenericType ? typeof(Enumerable).IsAssignableFrom(returnType.GetGenericTypeDefinition()) || typeof(ICollection).IsAssignableFrom(returnType.GetGenericTypeDefinition()) :
                typeof(Enumerable).IsAssignableFrom(returnType);

                var responseType = typeof(LinqToSolrResponse<>).MakeGenericType(
                    isCollection ? typeof(TObject).GetGenericTypeDefinition().MakeGenericType(ElementType)
                    : ElementType
                    );

                var sendMethod = GetType().GetMethod(nameof(SendAsync), BindingFlags.NonPublic | BindingFlags.Instance).MakeGenericMethod(responseType);
                var task = (Task)sendMethod.Invoke(this, new[] { request });
                await task.ConfigureAwait(false);
                var result = task.GetType().GetProperty(nameof(Task<object>.Result)).GetValue(task);
            }

            return await SendAsync<TObject>(request);
        }

        internal async Task<LinqToSolrResponse<TObject>> SendAsync<TObject>(LinqToSolrRequest request)
        {
            using (var client = new HttpClient())
            {
                var contentType = "application/json";
                var url = request.GetCoreUri();
                var uriBuilder = new UriBuilder(url);
                uriBuilder.Query = request.QueryParameters.ToString();

                var httpRequestMessage = new HttpRequestMessage(request.Method, uriBuilder.Uri)
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
    }
}