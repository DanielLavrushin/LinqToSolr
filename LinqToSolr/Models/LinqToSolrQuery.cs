using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

using LinqToSolr.Helpers.Json;
using LinqToSolr.Interfaces;
using LinqToSolr.Query;

#if NET35
using LinqToSolr.Expressions;
#endif

namespace LinqToSolr.Models
{
    public class LinqToSolrQuery : ILinqToSolrQuery
    {
        public ILinqToSolrProvider Provider { get; }
        public Expression Expression { get { return Query.Expression; } }
        public ICollection<ILinqToSolrFilter> Filters { get; }
        public ICollection<ILinqToSolrFacet> Facets { get; }
        public ICollection<ILinqToSolrFacet> FacetsToIgnore { get; }
        public ICollection<ILinqToSolrSort> Sortings { get; }
        public ICollection<ILinqToSolrJoiner> JoinFields { get; }

        public string Index { get; set; }
        internal string FilterUrl { get; set; }


        public ICollection<ILinqToSolrGrouping> GroupFields { get; }
        public ILinqSolrSelect Select { get; set; }
        public int Take { get; set; }
        public int Start { get; set; }
        IQueryable Query { get; }
        public LinqToSolrQuery(IQueryable query)
        {
            Query = query;
            Provider = query.Provider as ILinqToSolrProvider;
            Filters = new List<ILinqToSolrFilter>();
            Facets = new List<ILinqToSolrFacet>();
            FacetsToIgnore = new List<ILinqToSolrFacet>();
            Sortings = new List<ILinqToSolrSort>();
            GroupFields = new List<ILinqToSolrGrouping>();
            JoinFields = new List<ILinqToSolrJoiner>();
        }

    }
}