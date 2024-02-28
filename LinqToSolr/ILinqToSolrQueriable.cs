using LinqToSolr.Providers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace LinqToSolr
{
    public interface ILinqToSolrQueriable<TObject> : IOrderedQueryable<TObject>
    {
    }
}
