﻿using LinqToSolr.Providers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
#if !NETSTANDARD1_0
using System.Net.Http;
#endif
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

namespace LinqToSolr.Extensions
{
    public static class LinqExtensions
    {

        public static async Task<bool> DeleteAsync<TSource>(this IQueryable<TSource> query)
        {
            if (query.Provider is ILinqToSolrProvider provider)
            {
                var result = await provider.DeleteAsync(query) as LinqToSolrDeleteResponse<TSource>;
                return result.Success;
            }
            else
            {
                throw new InvalidOperationException("The provider is not supported for async operations.");
            }
        }

        public static async Task<bool> AddOrUpdateAsync<TSource>(this IQueryable<TSource> query, params TSource[] documents)
        {
            if (query.Provider is ILinqToSolrProvider provider)
            {
                var result = await provider.AddOrUpdateAsync(documents) as LinqToSolrUpdateResponse<TSource>;
                return result.Success;
            }
            else
            {
                throw new InvalidOperationException("The provider is not supported for async operations.");
            }
        }

        public static async Task<IDictionary<Expression<Func<T, object>>, object[]>> ToFacetsAsync<T>(this IQueryable<T> query, params Expression<Func<T, object>>[] expression)
        {
            if (query.Provider is ILinqToSolrProvider provider)
            {
                if (provider.Request == null)
                {
                    provider.Request = provider.PrepareRequest<T>(query.Expression);
                }
                foreach (var expr in expression)
                {
                    provider.Request.Translated.AddFacet(expr);
                }

                var result = await provider.ExecuteAsync<IDictionary<Expression<Func<T, object>>, object[]>>(query.Expression, provider.Request);
                return result;
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

        public static IQueryable<TSource> AsPostMethod<TSource>(this IQueryable<TSource> query)
        {
            if (query.Provider is ILinqToSolrProvider provider)
            {
                if (provider.Request == null)
                {
                    provider.Request = provider.PrepareRequest<TSource>(query.Expression);
                }
                provider.Request.Translated.Method = HttpMethod.Post;
                return query;
            }
            else
            {
                throw new InvalidOperationException("The provider is not supported for async operations.");
            }
        }
    }
}
