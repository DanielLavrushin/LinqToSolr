using System.Linq.Expressions;
using System.Reflection;
using Newtonsoft.Json;

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
                var dataMemberAttribute = fb.Member.GetCustomAttribute<JsonPropertyAttribute>();

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
