using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using Newtonsoft.Json;

namespace LinqToSolr.Data
{
    public class LinqToSolrFilter
    {
        public string Name { get; set; }
        public ICollection<object> Values { get; set; }

        public static LinqToSolrFilter Create<T>(string field, params object[] values)
        {
            var o = new LinqToSolrFilter();
            var prop = typeof(T).GetProperty(field);
            var dataMemberAttribute = prop.GetCustomAttribute<JsonPropertyAttribute>();

            o.Name = !string.IsNullOrEmpty(dataMemberAttribute?.PropertyName)
                   ? dataMemberAttribute.PropertyName
                   : prop.Name;

            o.Values = values.ToArray();

            return o;
        }

        public static LinqToSolrFilter Create(Type objectType, string field, params object[] values)
        {
            var o = new LinqToSolrFilter();
            var prop = objectType.GetProperty(field);
            var dataMemberAttribute = prop.GetCustomAttribute<JsonPropertyAttribute>();

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
                var dataMemberAttribute = fb.Member.GetCustomAttribute<JsonPropertyAttribute>();

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
