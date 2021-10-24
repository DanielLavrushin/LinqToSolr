using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

using LinqToSolr.Models;
using LinqToSolr.Expressions;
using LinqToSolr.Interfaces;

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

        public IEnumerable<TObject> Delete(IEnumerable<TObject> entities)
        {
            throw new NotImplementedException();
        }
    }
}
