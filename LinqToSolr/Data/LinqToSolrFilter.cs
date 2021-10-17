using System;
using System.Collections.Generic;
using System.Reflection;
using System.Linq;
using System.Linq.Expressions;
using LinqToSolr.Helpers.Json;
using LinqToSolr.Interfaces;
using LinqToSolr.Helpers;

namespace LinqToSolr.Data
{
    public class LinqToSolrFilter : ILinqToSolrFilter
    {
        public string Field { get; set; }
        public ICollection<object> Values { get; set; }
        public static LinqToSolrFilter Create(string fq)
        {
            var o = new LinqToSolrFilter
            {
                Field = fq
            };
            return o;
        }

        public static LinqToSolrFilter Create<T>(string field, params object[] values)
        {
            return Create(typeof(T), field, values);
        }

        public static LinqToSolrFilter Create(Type objectType, string field, params object[] values)
        {
            var prop = objectType.GetRuntimeProperty(field);
            var o = new LinqToSolrFilter
            {
                Field = prop.GetSolrFieldName(),
                Values = values.ToArray()
            };
            return o;
        }

        public static LinqToSolrFilter Create(LambdaExpression fieldExp, params object[] values)
        {
            var fb = fieldExp.Body as MemberExpression;

            var o = new LinqToSolrFilter
            {
                Field = fb.Member.GetSolrFieldName(),
                Values = values.ToArray()
            };
            return o;
        }
    }
}
