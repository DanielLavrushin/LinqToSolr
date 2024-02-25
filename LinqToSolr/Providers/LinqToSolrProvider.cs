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
            var task = InvokeGenericMethod(typeof(Task), nameof(ExecuteAsync), new[] { ElementType }, this, new[] { expression });
            return task.GetType().GetProperty("Result").GetValue(task);
        }

        public TResult Execute<TResult>(Expression expression)
        {
            return ExecuteAsync<TResult>(expression).GetAwaiter().GetResult();
        }

        public async Task<TResult> ExecuteAsync<TResult>(Expression expression)
        {
            var translator = new ExpressionTranslator<TResult>(expression);
            var translatedQuery = new TranslatedQuery();
            var query = translator.Translate(expression, translatedQuery);
            var request = new LinqToSolrRequest(this, query, HttpMethod.Get);
            var response = await PrepareAndSendAsync<TResult>(request);
            var docs = response.Response.Result;
            return docs;
        }

        internal async Task<LinqToSolrResponse<TObject>> PrepareAndSendAsync<TObject>(LinqToSolrRequest request)
        {
            var response = await PrepareAndSendAsync(request, typeof(TObject));
            return response as LinqToSolrResponse<TObject>;
        }

        internal async Task<object> PrepareAndSendAsync(LinqToSolrRequest request, Type returnType)
        {
            if (request.Translated.IsSelect)
            {
                var response = await SendAsync(request, returnType);
                var documents = SelectDocuments(request, response);
                var selectresponse = CreateFakeResponse(documents, returnType);

                return selectresponse;
            }
            return await SendAsync(request, returnType);
        }
        internal Task<LinqToSolrResponse<TObject>> SendAsync<TObject>(LinqToSolrRequest request)
        {
            return SendAsync(request, typeof(TObject)) as Task<LinqToSolrResponse<TObject>>;
        }

        internal async Task<object> SendAsync(LinqToSolrRequest request, Type returnType)
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

                var isCollection = returnType.IsGenericType ? typeof(Enumerable).IsAssignableFrom(returnType.GetGenericTypeDefinition()) || typeof(ICollection).IsAssignableFrom(returnType.GetGenericTypeDefinition()) : typeof(Enumerable).IsAssignableFrom(returnType);
                var responseType = typeof(LinqToSolrResponse<>).MakeGenericType(isCollection ? returnType.GetGenericTypeDefinition().MakeGenericType(ElementType) : ElementType);
                var response = JsonParser.FromJson(responseContent, responseType);
                Debug.WriteLine(uriBuilder.Uri);
                return response;
            }
        }

        object CreateFakeResponse(object documents, Type returnType)
        {
            if (returnType == null) throw new ArgumentNullException(nameof(returnType));
            var responseGenericType = typeof(LinqToSolrResponse<>).MakeGenericType(returnType);
            var response = Activator.CreateInstance(responseGenericType);
            response.GetType().GetProperty("Response").PropertyType.GetProperty("Result").SetValue(response.GetType().GetProperty("Response").GetValue(response), documents);
            return response;
        }

        object SelectDocuments(LinqToSolrRequest request, object response)
        {
            if (request == null) throw new ArgumentNullException(nameof(request));
            if (response == null) throw new ArgumentNullException(nameof(response));
            var documentsType = response.GetType().GetProperty("Response").PropertyType.GetProperty("Result").PropertyType;
            var elementType = documentsType.GetGenericArguments()[0];
            var documents = response.GetType().GetProperty("Response").GetValue(response).GetType().GetProperty("Result").GetValue(response.GetType().GetProperty("Response").GetValue(response));

            var sourceType = documents.GetType().GetGenericArguments()[0];
            var resultType = request.Translated.SelectExpression.Body.Type;
            var func = request.Translated.SelectExpression.Compile();

            var selectedDocuments = InvokeGenericMethod(typeof(Enumerable), nameof(Enumerable.Select), new[] { sourceType, resultType }, null, new[] { documents, func });
            var materializedResult = InvokeGenericMethod(typeof(Enumerable), nameof(Enumerable.ToList), new[] { resultType }, null, new[] { selectedDocuments });

            return materializedResult;
        }
        private object InvokeGenericMethod(Type type, string methodName, Type[] genericTypes, object target, object[] parameters)
        {
            var method = type.GetMethods().First(m => m.Name == methodName && m.GetGenericArguments().Length == genericTypes.Length).MakeGenericMethod(genericTypes);
            return method.Invoke(target, parameters);
        }

    }
}