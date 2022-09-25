using System;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

using LinqToSolr.Helpers.Json;
using LinqToSolr.Interfaces;
using LinqToSolr.Helpers;

namespace LinqToSolr.Models
{
    public enum SolrSortTypes
    {
        Asc,
        Desc
    }
    public class LinqToSolrSort : ILinqToSolrSort
    {
        public string Field { get; set; }
        public SolrSortTypes Order { get; set; }

        public static LinqToSolrSort Create(Expression fieldExp, SolrSortTypes order)
        {
            var o = new LinqToSolrSort();
            var fb = fieldExp as MemberExpression;
            if (fieldExp is UnaryExpression)
            {
                var ufb = ((UnaryExpression)fieldExp);
                fb = ufb.Operand as MemberExpression;
            }
            if (fb != null)
            {
                o.Field = fb.Member.GetSolrFieldName();
                o.Order = order;
                return o;
            }
            return null;
        }
    }
}
