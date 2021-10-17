using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;

using LinqToSolr.Query;

namespace LinqToSolr.Interfaces
{
    public interface ILinqToSolrQuery
    {
        ILinqToSolrProvider Provider { get; }
        Expression Expression { get; }
        ICollection<ILinqToSolrFilter> Filters { get; }
        ICollection<ILinqToSolrFacet> Facets { get; }
        ICollection<ILinqToSolrFacet> FacetsToIgnore { get; }
        ICollection<ILinqToSolrSort> Sortings { get; }
        ICollection<ILinqToSolrJoiner> JoinFields { get; }
        ICollection<string> GroupFields { get; }
        ILinqSolrSelect Select { get; set; }

        int Take { get; set; }
        int Start { get; set; }


    }
}
