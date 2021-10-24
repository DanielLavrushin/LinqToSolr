using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;

using LinqToSolr.Models;
using LinqToSolr.Expressions;
using LinqToSolr.Interfaces;
using LinqToSolr.Query;

namespace LinqToSolr
{
    public static class Extensions
    {
        public static IQueryable<TSource> ExcludeFacets<TSource>(this IQueryable<TSource> enumerable, Expression<Func<TSource, object>> expression)
        {
            var query = enumerable as ILinqToSolrQueriable<TSource>;
            query.SolrQuery.FacetsToIgnore.Add(LinqToSolrFacet.Create(expression));
            return enumerable;
        }

        public static ILinqToSolrFacet<TSource> ToFacets<TSource>(this IQueryable<TSource> enumerable, params Expression<Func<TSource, object>>[] expression)
        {
            var query = enumerable as ILinqToSolrQueriable<TSource>;
            if (query == null) throw new ArgumentException("ToFacets must be invoked from ILinqToSolrQueriable<>");
            var provider = (ILinqToSolrProvider)query.Provider;

            foreach (var expr in expression)
            {
                query.SolrQuery.Facets.Add(LinqToSolrFacet.Create(expr));
            }
            var result = provider.ExecuteQuery(query);
            return new LinqToSolrFacet<TSource>(result);
        }

        public static IQueryable<TSource> Include<TSource>(this IQueryable<TSource> enumerable, params string[] properties)
        {
            if (properties.Any())
            {
                var query = enumerable as LinqToSolrQueriable<TSource>;
                if (query == null) throw new ArgumentException("ToFacets must be invoked from ILinqToSolrQueriable<>");

                foreach (var prop in properties)
                {
                    var joiner = new LinqToSolrJoiner(prop, typeof(TSource));
                    query.SolrQuery.JoinFields.Add(joiner);

                }
            }
            return enumerable;
        }
        public static void Delete<TSource>(this IQueryable<TSource> enumerable, params object[] id)
        {
            var query = enumerable as ILinqToSolrQueriable<TSource>;
            if (query == null) throw new ArgumentException("ToFacets must be invoked from  ILinqToSolrQueriable<>");

            var provider = (ILinqToSolrProvider)query.Provider;
            provider.Delete<TSource>(id);
        }
        public static void Delete<TSource>(this IQueryable<TSource> enumerable, Expression<Func<TSource, bool>> expression)
        {
            var query = enumerable as ILinqToSolrQueriable<TSource>;
            if (query == null) throw new ArgumentException("ToFacets must be invoked from ILinqToSolrQueriable<>");

            var translator = new LinqToSolrQueryTranslator(query.SolrQuery);
            var q = Evaluator.PartialEval(expression);
            var strQuery = translator.Translate(BooleanVisitor.Process(q));
            query.SolrQuery.Filters.Clear();
            query.SolrQuery.Filters.Add(LinqToSolrFilter.Create(strQuery));
            var provider = (ILinqToSolrProvider)query.Provider;
            provider.Delete(query);
        }
    }
}