using System;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using Newtonsoft.Json;

namespace LinqToSolr.Data
{
    public class LinqToSolrFacet
    {
        public string SolrName { get; set; }
        public string Name { get; set; }
        public LambdaExpression Property { get; set; }

        public static LinqToSolrFacet Create(LambdaExpression fieldExp)
        {
            var o = new LinqToSolrFacet { Property = fieldExp };

            var fb = fieldExp.Body as MemberExpression;
            if (fb != null)
            {
#if NET40 || NET35 || PORTABLE40
                var dataMemberAttribute =
                    Attribute.GetCustomAttribute(fb.Member, typeof(JsonPropertyAttribute), true) as
                        JsonPropertyAttribute;
#else

                var dataMemberAttribute = fb.Member.GetCustomAttribute<JsonPropertyAttribute>();
#endif

                o.SolrName = !string.IsNullOrEmpty(dataMemberAttribute?.PropertyName)
                    ? dataMemberAttribute.PropertyName
                    : fb.Member.Name;

                o.Name = fb.Member.Name;
                return o;
            }

            return null;
        }

    }
}
