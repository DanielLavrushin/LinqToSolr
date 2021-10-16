using System;
using System.Collections.Generic;
using System.Reflection;
using System.Linq;
using System.Linq.Expressions;
using LinqToSolr.Helpers.Json;

namespace LinqToSolr.Data
{
    public class LinqToSolrFilter
    {
        public string Name { get; set; }
        public ICollection<object> Values { get; set; }

        public static LinqToSolrFilter Create<T>(string field, params object[] values)
        {
            var o = new LinqToSolrFilter();

#if NETSTANDARD
            var prop = typeof(T).GetRuntimeProperty(field);

#else
            var prop = typeof(T).GetProperty(field);
#endif

#if NET45_OR_GREATER
                var dataMemberAttribute = prop.GetCustomAttribute<JsonPropertyAttribute>();
#else
            var dataMemberAttribute = prop.GetCustomAttributes(typeof(JsonPropertyAttribute), true).FirstOrDefault() as JsonPropertyAttribute;

#endif

            o.Name = !string.IsNullOrEmpty(dataMemberAttribute?.PropertyName)
                ? dataMemberAttribute.PropertyName
                : prop.Name;

            o.Values = values.ToArray();

            return o;
        }

        public static LinqToSolrFilter Create(Type objectType, string field, params object[] values)
        {
            var o = new LinqToSolrFilter();

#if NETSTANDARD
            var prop = objectType.GetRuntimeProperty(field);

#else
            var prop = objectType.GetProperty(field);
#endif

#if NET45_OR_GREATER
                var dataMemberAttribute = prop.GetCustomAttribute<JsonPropertyAttribute>();
#else
            var dataMemberAttribute = prop.GetCustomAttributes(typeof(JsonPropertyAttribute), true).FirstOrDefault() as JsonPropertyAttribute;

#endif

            o.Name = !string.IsNullOrEmpty(dataMemberAttribute?.PropertyName)
                   ? dataMemberAttribute.PropertyName
                   : prop.Name;


            o.Values = values.ToArray();
            return o;
        }

        public static LinqToSolrFilter Create(LambdaExpression fieldExp, params object[] values)
        {
            var o = new LinqToSolrFilter();

            var fb = fieldExp.Body as MemberExpression;
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


                o.Values = values.ToArray();


                return o;
            }

            return null;
        }
    }
}
