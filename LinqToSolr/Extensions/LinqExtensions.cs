using LinqToSolr.Providers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

namespace LinqToSolr.Extensions
{
    public static class LinqExtensions
    {
        public static IQueryable<T> GroupByFacets<T>(this IQueryable<T> query, params Expression<Func<T, object>>[] expression)
        {
            if (query.Provider is ILinqToSolrProvider provider)
            {
                foreach (var expr in expression)
                {
                    //   query.SolrQuery.Facets.Add(LinqToSolrFacet.Create(expr));
                }
                return query;
            }
            else
            {
                throw new InvalidOperationException("The provider is not supported for async operations.");
            }
        }
        public static async Task<List<T>> ToListAsync<T>(this IQueryable<T> query)
        {
            if (query.Provider is ILinqToSolrProvider provider)
            {
                var expression = query.Expression;
                var result = await provider.ExecuteAsync<List<T>>(expression);
                return result;
            }
            else
            {
                throw new InvalidOperationException("The provider is not supported for async operations.");
            }
        }

        public static async Task<T> FirstOrDefaultAsync<T>(this IQueryable<T> query)
        {
            if (query.Provider is ILinqToSolrProvider provider)
            {
                var expression = query.Expression;
                var result = await provider.ExecuteAsync<T>(expression);
                return result;
            }
            else
            {
                throw new InvalidOperationException("The provider is not supported for async operations.");
            }

        }
    }
}
