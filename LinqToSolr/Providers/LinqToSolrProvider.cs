using LinqToSolr.Expressions;
using LinqToSolr.Extensions;
using System.Reflection;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;
using System.Net;
#if NETSTANDARD1_1_OR_GREATER
using System.Net.Http;
using System.Net.Http.Headers;
#endif
using System.Text;
using System.Threading.Tasks;

namespace LinqToSolr.Providers
{
    public class LinqToSolrProvider : ILinqToSolrProvider, IDisposable
    {
        private static readonly HttpClient httpClient = new HttpClient();
        public Type ElementType { get; }
        public ILinqToSolrService Service { get; }

        public ITranslatedQuery Translated { get; }

        public LinqToSolrProvider(ILinqToSolrService service, Type elementType)
        {
            Service = service;
            ElementType = elementType;
            if (Service.Configuration.Endpoint.IsProtected)
            {
                var byteArray = NetStandardSupport.GetAsciiBytes($"{Service.Configuration.Endpoint.Username}:{Service.Configuration.Endpoint.Password}");
                httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", Convert.ToBase64String(byteArray));
            }
            Translated = new TranslatedQuery();
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
            var query = translator.Translate(expression, Translated);
            var request = new LinqToSolrRequest(this, query, HttpMethod.Get);
            var response = await PrepareAndSendAsync<TResult>(request);
            var docs = response.GetDocuments();
            return docs;
        }

        internal async Task<ILinqToSolrFinalResponse<TObject>> PrepareAndSendAsync<TObject>(LinqToSolrRequest request)
        {
            var response = await PrepareAndSendAsync(request, typeof(TObject));
            return response as ILinqToSolrFinalResponse<TObject>;
        }

        internal async Task<object> PrepareAndSendAsync(LinqToSolrRequest request, Type returnType)
        {
            var response = await SendAsync(request, returnType);
            if (request.Translated.Select.Any())
            {
                var documents = SelectDocuments(request, response);
                var selectresponse = CreateFakeResponse(documents, returnType);
                return selectresponse;
            }

            if (request.Translated.Groups.Any())
            {
                var selectresponse = CreateFakeResponse(response, returnType);
                return selectresponse;
            }

            if (request.Translated.Facets.Any())
            {
                var selectresponse = CreateFakeResponse(response, returnType);
                return selectresponse;
            }

            return response;
        }

        internal Task<ILinqToSolrFinalResponse<TObject>> SendAsync<TObject>(LinqToSolrRequest request)
        {
            return SendAsync(request, typeof(TObject)) as Task<ILinqToSolrFinalResponse<TObject>>;
        }
        internal async Task<object> SendAsync(LinqToSolrRequest request, Type returnType)
        {
            var url = request.GetCoreUri();
            var uriBuilder = new UriBuilder(url);
            uriBuilder.Query = request.QueryParameters.ToString();


            var httpRequestMessage = new HttpRequestMessage(request.Method, uriBuilder.Uri)
            {
                Content = (request.Method != HttpMethod.Get ? new StringContent(uriBuilder.Query, Encoding.UTF8, "application/x-www-form-urlencoded") : null)
            };
            var httpResponse = await httpClient.SendAsync(httpRequestMessage);
            var responseContent = await httpResponse.Content.ReadAsStringAsync();

            if (httpResponse.StatusCode != HttpStatusCode.OK)
            {
                throw LinqToSolrException.ParseSolrErrorResponse(responseContent);
            }

            var response = JsonParser.FromJson(responseContent, GetResponseType(returnType, request));

            if (request.Translated.Groups.Any())
            {
                var keyType = returnType.GenericTypeArguments[0].GenericTypeArguments[0];
                var listType = typeof(List<>).MakeGenericType(ElementType);
                response = InvokeGenericMethod(GetType(), nameof(GroupDocuments), new[] { keyType, ElementType, listType }, this, new[] { response, returnType });
            }

            if (request.Translated.Facets.Any())
            {
                var facetType = returnType.GenericTypeArguments[0];
                var listType = typeof(List<>).MakeGenericType(facetType);
                response = InvokeGenericMethod(GetType(), nameof(FaceDocuments), new[] { returnType, ElementType }, this, new[] { response, request });
            }

            Debug.WriteLine(uriBuilder.Uri);
            return response;
        }

        object CreateFakeResponse(object documents, Type returnType, Type responseType = null)
        {
            if (returnType == null) throw new ArgumentNullException(nameof(returnType));

            responseType = responseType ?? typeof(LinqToSolrResponse<>);
            var responseGenericType = responseType.MakeGenericType(returnType);
            var response = Activator.CreateInstance(responseGenericType);
            response.GetType().GetProperty("Response").PropertyType.GetProperty("Result").SetValue(response.GetType().GetProperty("Response").GetValue(response), documents);
            return response;
        }
        Type GetResponseType(Type returnType, LinqToSolrRequest request)
        {
            if (returnType == null) throw new ArgumentNullException(nameof(returnType));

            var responseGenericType = request.Translated.Facets.Any() ? typeof(LinqToSolrFacetsResponse<>) : request.Translated.Groups.Any() ? typeof(LinqToSolrGroupResponse<>) : typeof(LinqToSolrResponse<>);
            var baseElementType = request.Translated.Facets.Any() ? returnType : returnType.IsGenericType() ? returnType.GetGenericTypeDefinition().MakeGenericType(ElementType) : ElementType;

            var responseType = responseGenericType.MakeGenericType(baseElementType);
            return responseType;
        }

        private object FaceDocuments<TObject, TElement>(LinqToSolrFacetsResponse<TObject> response, LinqToSolrRequest request)
        {
            var dict = new LinqToSolrExpressionDictionary<TElement>();

            foreach (var facetField in response.Result.FacetFields)
            {
                var expressionKey = request.Translated.Facets[facetField.Key] as Expression<Func<TElement, object>>;
                var propertyType = GetPropertyTypeFromExpression(expressionKey);
                var values = new List<object>();
                for (int i = 0; i < facetField.Value.Length; i += 2)
                {
                    var value = Convert.ChangeType(facetField.Value[i], propertyType);
                    values.Add(value);
                }
                dict.Add(expressionKey, values.ToArray());
            }

            return dict;
        }
        private Type GetPropertyTypeFromExpression<TElement>(Expression<Func<TElement, object>> expression)
        {
            if (expression.Body is UnaryExpression unaryExpression && unaryExpression.Operand is MemberExpression memberExpression)
            {
                return ((PropertyInfo)memberExpression.Member).PropertyType;
            }
            else if (expression.Body is MemberExpression memberExpression2)
            {
                return ((PropertyInfo)memberExpression2.Member).PropertyType;
            }
            throw new InvalidOperationException("Could not determine property type from expression.");
        }
        private object GroupDocuments<TKey, TElement, TObject>(LinqToSolrGroupResponse<TObject> response, Type returnType) where TObject : IEnumerable<TElement>
        {
            var groups = new List<IGrouping<TKey, TElement>>();

            foreach (var groupField in response.Grouped)
            {
                foreach (var group in groupField.Value.Groups)
                {
                    var key = (TKey)group.Id;
                    var elements = group.Result.Documents;

                    var grouping = new Grouping<TKey, TElement, TObject>(key, elements);
                    groups.Add(grouping);
                }
            }

            var result = Convert.ChangeType(groups, returnType);
            return result;
        }
        private object SelectDocuments(LinqToSolrRequest request, object response)
        {
            if (request == null) throw new ArgumentNullException(nameof(request));
            if (response == null) throw new ArgumentNullException(nameof(response));
            var genericResponse = response as ILinqToSolrFinalResponse<object>;
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

        internal class Grouping<TKey, TElement, TObject> : IGrouping<TKey, TElement> where TObject : IEnumerable<TElement>
        {
            private readonly TKey _key;
            private readonly TObject _elements;

            public Grouping(TKey key, TObject elements)
            {
                _key = key;
                _elements = elements;
            }
            public TKey Key => _key;
            public IEnumerator<TElement> GetEnumerator() => _elements.GetEnumerator();
            IEnumerator IEnumerable.GetEnumerator() => _elements.GetEnumerator();
        }

        private object InvokeGenericMethod(Type type, string methodName, Type[] genericTypes, object target, object[] parameters)
        {
            var flags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static;
            var method = type.GetMethods(flags).First(m => m.Name == methodName && m.GetGenericArguments().Length == genericTypes.Length).MakeGenericMethod(genericTypes);
            return method.Invoke(target, parameters);
        }

        public void Dispose()
        {

        }
    }
}