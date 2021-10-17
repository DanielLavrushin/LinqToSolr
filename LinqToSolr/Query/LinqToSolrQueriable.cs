using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

using LinqToSolr.Data;
using LinqToSolr.Expressions;
using LinqToSolr.Interfaces;
using LinqToSolr.Services;

namespace LinqToSolr.Query
{
    public interface ILinqToSolrQueriable<TObject> : IOrderedQueryable<TObject>
    {
        ILinqToSolrQuery SolrQuery { get; }
        ILinqToSolrQuery Translate();
    }
    public class LinqToSolrQueriable<TObject> : ILinqToSolrQueriable<TObject>
    {
        public IQueryProvider Provider { get; }
        public Expression Expression { get; }
        public Type ElementType
        {
            get { return typeof(TObject); }
        }
        public ILinqToSolrQuery SolrQuery { get; }

        public LinqToSolrQueriable(ILinqToSolrProvider provider, Expression expression)
        {
            Provider = provider;
            Expression = expression ?? Expression.Constant(this);
            SolrQuery = new LinqToSolrQuery(this);
        }

        public IEnumerator<TObject> GetEnumerator()
        {
            return Provider.Execute<IEnumerable<TObject>>(Expression).GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return Provider.Execute<IEnumerable>(Expression).GetEnumerator();
        }


        public ILinqToSolrQuery Translate()
        {
            var qt = new LinqToSolrQueryTranslator(SolrQuery);
            var expression = Evaluator.PartialEval(Expression, e => e.NodeType != ExpressionType.Parameter && !typeof(IQueryable).IsAssignableFrom(e.Type));
            qt.Translate(BooleanVisitor.Process(expression));
            return SolrQuery;
        }
    }

    public static class SolrQuaryableExtensions
    {
        public static IQueryable<TSource> Include<TSource>(this IQueryable<TSource> enumerable, params string[] properties)
        {
            if (properties.Any())
            {
                var query = enumerable as LinqToSolrQueriable<TSource>;
                if (query == null) throw new ArgumentException("'Include' must be invoked as SolrQueryable extension");

                foreach (var prop in properties)
                {
                    var joiner = new LinqToSolrJoiner(prop, typeof(TSource));
                    query.SolrQuery.JoinFields.Add(joiner);

                }
            }
            return enumerable;
        }

        public static IQueryable<TSource> ExcludeFacetFromQuery<TSource>(this IQueryable<TSource> enumerable,
            Expression<Func<TSource, object>> expression)
        {
            var query = enumerable as LinqToSolrQueriable<TSource>;
            query.SolrQuery.FacetsToIgnore.Add(LinqToSolrFacet.Create(expression));
            return enumerable;
        }

        public static IEnumerable<IGrouping<string, TKey>> GroupByFacets<TSource, TKey>(this IQueryable<TSource> enumerable, params Expression<Func<TSource, TKey>>[] expression)
        {
            return GroupByFacets(enumerable, 0, expression);
        }

        public static IEnumerable<IGrouping<string, TKey>> GroupByFacets<TSource, TKey>(this IQueryable<TSource> enumerable, int facetsLimit, params Expression<Func<TSource, TKey>>[] expression)
        {
            var query = enumerable as LinqToSolrQueriable<TSource>;
            if (query == null) throw new ArgumentException("GroupBySolr must be invoked as SolrQueryable extension");
            var service = ((LinqToSolrProvider)query.Provider).Service;

            foreach (var expr in expression)
            {
                query.SolrQuery.Facets.Add(LinqToSolrFacet.Create(expr));
            }
            var oldStart = service.Configuration.Start;
            var oldTake = service.Configuration.Take;

            service.Configuration.Start = 0;
            service.Configuration.Take = 0;
            if (facetsLimit > 0)
            {
                service.Configuration.FacetsLimit = facetsLimit;
            }
            var result = query.ToList();

            service.Configuration.Start = oldStart;
            service.Configuration.Take = oldTake;

            ////var items = service.LastResponse.Facets.Fields;
            ////var groups = items.SelectMany(x => x.Value.Select(k => new { x.Key, Value = k }))
            ////    .ToLookup(x => x.Key, x => (TKey)x.Value);
            return null;
        }


    }
}
