using System;
using System.Linq.Expressions;

namespace LinqToSolr.Interfaces
{
    public interface ILinqSolrSelect
    {
        Type Type { get; }
        Expression Expression { get; }
        string GetSelectFields();
    }
}
