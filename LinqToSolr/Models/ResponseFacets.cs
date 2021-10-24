using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;

namespace LinqToSolr.Interfaces
{
    public class ResponseFacets<TResult>
    {
        [SolrField("facet_fields")]
        Dictionary<string, object[]> Fields { get; set; }

        public IDictionary<TKey, int> Get<TKey>(Expression<Func<TResult, TKey>> prop)
        {
            if (prop.Body.NodeType == ExpressionType.MemberAccess)
            {
                var member = ((MemberExpression)prop.Body).Member;
                var fieldName = SolrFieldAttribute.GetFieldName(member);

                if (Fields.ContainsKey(fieldName))
                {
                    var dict = new Dictionary<TKey, int>();
                    var elem = Fields[fieldName];
                    var keys = elem.Where((i, index) => (index & 1) == 0).ToArray();
                    var vals = elem.Where((i, index) => (index & 1) == 1).ToArray();
                    for (int i = 0; i < keys.Length; i++)
                    {
                        dict.Add((TKey)keys[i], (int)vals[i]);
                    }
                    return dict;
                }

            }
            return null;
        }

    }
}
