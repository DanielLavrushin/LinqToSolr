using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

using LinqToSolr.Helpers;
using LinqToSolr.Helpers.Json;
using LinqToSolr.Interfaces;

namespace LinqToSolr.Data
{

    public class LinqToSolrFacet : ILinqToSolrFacet
    {
        public string Field { get; set; }
        public LambdaExpression Property { get; set; }

        public static LinqToSolrFacet Create(LambdaExpression fieldExp)
        {
            var o = new LinqToSolrFacet { Property = fieldExp };

            var fb = fieldExp.Body as MemberExpression;
            if (fb != null)
            {
                o.Field = fb.Member.GetSolrFieldName();
                return o;
            }

            return null;
        }

    }
}
