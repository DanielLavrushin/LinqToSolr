using LinqToSolr.Expressions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;

namespace LinqToSolr.Providers
{
    public class LinqToSolrProvider : ILinqToSolrProvider
    {
        public IQueryable CreateQuery(Expression expression)
        {
            throw new NotImplementedException();
        }

        public IQueryable<TElement> CreateQuery<TElement>(Expression expression)
        {
            return new LinqToSolrQueriable<TElement>(this, expression);
        }

        public object Execute(Expression expression)
        {
            return null;
            throw new NotImplementedException();
        }

        public TResult Execute<TResult>(Expression expression)
        {
            var translator = new ExpressionTranslator<TResult>(expression);
            translator.Translate(expression);
            return (TResult)Execute(expression);
        }
    }
}
