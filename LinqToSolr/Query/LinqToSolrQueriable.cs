using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using LinqToSolr.Data;
using LinqToSolr.Services;

namespace LinqToSolr.Query
{
    public class LinqToSolrQueriable<TObject>: IOrderedQueryable<TObject>
    {
        public LinqToSolrQueriable(ILinqToSolrService service)
        {
            service.ElementType = typeof(TObject);
            Provider = new LinqToSolrProvider(service);
            Expression = Expression.Constant(this);
        }
        public LinqToSolrQueriable(LinqToSolrProvider provider, Expression expression)
        {
            Provider = provider;
            Expression = expression;
        }

        public ILinqToSolrService GetSolrQuery()
        {
            return ((LinqToSolrProvider)Provider).GetSolrQuery(Expression);
        }

        public Expression Expression { get; }
        public Type ElementType
        {
            get { return typeof(TObject); }
        }
        public IQueryProvider Provider { get; }

        public IEnumerator<TObject> GetEnumerator()
        {
            return Provider.Execute<IEnumerable<TObject>>(Expression).GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return Provider.Execute<IEnumerable>(Expression).GetEnumerator();
        }



        #region LINQ Custom Solr Methods

        #endregion

    }

    public static class SolrQuaryableExtensions
    {
#if NETSTANDARD1_6
        public static IEnumerable<IGrouping<string, TKey>> GroupByFacets<TSource, TKey>(this IEnumerable<TSource> enumerable, params Expression<Func<TSource, TKey>>[] expression)
#else
        public static IEnumerable<IGrouping<string, TKey>> GroupByFacets<TSource, TKey>(this IQueryable<TSource> enumerable, params Expression<Func<TSource, TKey>>[] expression)
#endif
        {
            var query = enumerable as LinqToSolrQueriable<TSource>;
            if (query == null) throw new ArgumentException("GroupBySolr must be invoked as SolrQueryable extension");
            var service = ((LinqToSolrProvider)query.Provider).Service;
            service.CurrentQuery = service.CurrentQuery ?? new LinqToSolrQuery();
            foreach (var expr in expression)
            {
                service.CurrentQuery.AddFacet(expr);
            }
            var oldStart = service.Configuration.Start;
            var oldTake = service.Configuration.Take;

            service.Configuration.Start = 0;
            service.Configuration.Take = 0;

            var result = query.ToList();

            service.Configuration.Start = oldStart;
            service.Configuration.Take = oldTake;

            var items = service.LastResponse.Facets.Fields;
            var groups = items.SelectMany(x => x.Value.Select(k => new { x.Key, Value = k }))
                .ToLookup(x => x.Key, x => (TKey)x.Value);
            return groups;
        }


    }
}
