using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace LinqToSolr.Providers
{
    public interface ILinqToSolrProvider : IQueryProvider
    {
        Type ElementType { get; }
        ILinqToSolrService Service { get; }
        Task<TResult> ExecuteAsync<TResult>(Expression expression);
    }
}
