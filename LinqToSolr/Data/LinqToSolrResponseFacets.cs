using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using Newtonsoft.Json;

namespace LinqToSolr.Data
{
    public class LinqToSolrResponseFacets
    {
        internal Dictionary<string, IEnumerable<object>> Fields;

        public LinqToSolrResponseFacets()
        {
            Fields = new Dictionary<string, IEnumerable<object>>();
        }

        public bool Any()
        {
            return Fields.Any();
        }

        public void Add(string key, object[] values)
        {
            if (!Fields.ContainsKey(key))
            {
                Fields.Add(key, values);
            }
        }

        public IEnumerable<object> Get<T>(Expression<Func<T, object>> key)
        {
            var fb = key.Body as MemberExpression;
            if (fb != null)
            {
                var dataMemberAttribute =
                    fb.Member.GetCustomAttributes(typeof(JsonPropertyAttribute), false)[0] as JsonPropertyAttribute;

                var k = dataMemberAttribute != null && !string.IsNullOrEmpty(dataMemberAttribute.PropertyName)
                      ? dataMemberAttribute.PropertyName
                      : fb.Member.Name;

                return Fields[k];
            }
            return null;
        }

        public T Get<T>()
        {
            //do not use this - not ready
            var obj = Activator.CreateInstance<T>();

            var props = obj.GetType().GetProperties();

            foreach (var p in props)
            {
                if (Fields.ContainsKey(p.Name))
                {
                    var values = Fields[p.Name];

                    p.SetValue(obj, values, null);
                }
            }

            return obj;
        }
    }
}
