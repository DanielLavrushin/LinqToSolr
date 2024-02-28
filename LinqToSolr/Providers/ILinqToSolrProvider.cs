using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using LinqToSolr.Expressions;

namespace LinqToSolr.Providers
{
    public interface ILinqToSolrProvider : IQueryProvider
    {
        Type ElementType { get; }
        ILinqToSolrService Service { get; }
        ILinqToSolrRequest Request { get; set; }
        TResult Execute<TResult>(Expression expression, ILinqToSolrRequest request);
        Task<TResult> ExecuteAsync<TResult>(Expression expression);
        Task<TResult> ExecuteAsync<TResult>(Expression expression, ILinqToSolrRequest request);
        ILinqToSolrRequest PrepareRequest<TResult>(Expression expression);
        Task<ILinqToSolrFinalResponse<TSource>> AddOrUpdateAsync<TSource>(params TSource[] documents);
        Task<ILinqToSolrFinalResponse<TSource>> DeleteAsync<TSource>(IQueryable<TSource> query);
    }
}
