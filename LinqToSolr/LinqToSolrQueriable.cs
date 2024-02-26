using LinqToSolr.Expressions;
using LinqToSolr.Providers;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;

namespace LinqToSolr
{
    public class LinqToSolrQueriable<TObject> : ILinqToSolrQueriable<TObject>
    {
        public Type ElementType
        {
            get { return typeof(TObject); }
        }
        public Expression Expression { get; }
        public IQueryProvider Provider { get; }

        internal LinqToSolrQueriable(ILinqToSolrProvider provider) : this(provider, null)
        {
        }

        internal LinqToSolrQueriable(ILinqToSolrProvider provider, Expression expression)
        {
            Provider = provider;
            Expression = expression ?? Expression.Constant(this);
        }

        public IEnumerator<TObject> GetEnumerator()
        {
            return Provider.Execute<IEnumerable<TObject>>(Expression).GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}
