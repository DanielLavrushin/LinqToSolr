using LinqToSolr.Providers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LinqToSolr.Extensions
{
    public static class LinqExtensions
    {
        public static async Task<List<T>> ToListAsync<T>(this IQueryable<T> query)
        {
            if (query.Provider is ILinqToSolrProvider provider)
            {
                var expression = query.Expression;
                return null;
                //     var result = await provider.ExecuteAsync<T>(expression);
                //     return result.ToList();
            }
            else
            {
                throw new InvalidOperationException("The provider is not supported for async operations.");
            }
        }
    }
}
