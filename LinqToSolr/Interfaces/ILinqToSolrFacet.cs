using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace LinqToSolr.Interfaces
{

    public interface ILinqToSolrFacet
    {
        string Field { get; set; }
    }

    public interface ILinqToSolrFacet<TResult>
    {
        IEnumerable<TResult> Documents { get; }
        IDictionary<TKey, int> Get<TKey>(Expression<Func<TResult, TKey>> prop);
    }
}
