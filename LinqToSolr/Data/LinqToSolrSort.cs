using System;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

using LinqToSolr.Helpers.Json;

namespace LinqToSolr.Data
{

    public enum SolrSortTypes
    {
        Asc,
        Desc
    }


    public class LinqToSolrSort
    {
        public string Name { get; set; }
        public SolrSortTypes Order { get; set; }

        public static LinqToSolrSort Create(Expression fieldExp, SolrSortTypes order)
        {
            var o = new LinqToSolrSort();

            var fb = fieldExp as MemberExpression;
            if (fb != null)
            {
#if NET40 || NET35
                var dataMemberAttribute = Attribute.GetCustomAttribute(fb.Member, typeof(JsonPropertyAttribute), true) as JsonPropertyAttribute;
#else
                var dataMemberAttribute = fb.Member.GetCustomAttribute<JsonPropertyAttribute>();
#endif
                o.Name = !string.IsNullOrEmpty(dataMemberAttribute?.PropertyName)
                    ? dataMemberAttribute.PropertyName
                    : fb.Member.Name;


                o.Order = order;


                return o;
            }

            return null;
        }
    }
}
