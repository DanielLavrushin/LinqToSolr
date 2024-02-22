using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;

namespace LinqToSolr.Providers
{
    public interface ILinqToSolrProvider : IQueryProvider
    {
        ILinqToSolrService Service { get; }
    }
}
