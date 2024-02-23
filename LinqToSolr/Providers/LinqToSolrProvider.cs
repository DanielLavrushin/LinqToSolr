using LinqToSolr.Expressions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
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
            var task = (Task)executeMethod.Invoke(this, new object[] { expression });
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
            using (var client = new LinqToSolrHttpClient(this))
            {
                var request = new LinqToSolrRequest<TResult>(client, query, LinqToSolrHttpMethod.GET);
                var response = await client.Execute(request);
                var docs = response.Response.Result;
                return docs;
            }
        }
    }
}