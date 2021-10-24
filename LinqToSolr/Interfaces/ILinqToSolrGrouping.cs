using System;
using System.Linq.Expressions;

namespace LinqToSolr.Interfaces
{
    public interface ILinqToSolrGrouping
    {
        string Field { get; set; }
        Type Type { get; }
        Expression Expression { get; }

    }
}
