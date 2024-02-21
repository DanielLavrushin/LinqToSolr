using LinqToSolr.Providers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LinqToSolr
{
    public interface ILinqToSolrQueriable<TObject> : IOrderedQueryable<TObject>
    {
    }
}
