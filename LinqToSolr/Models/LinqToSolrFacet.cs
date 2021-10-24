﻿using System;
using System.Collections.Generic;
using System.Linq.Expressions;

using LinqToSolr.Helpers;
using LinqToSolr.Interfaces;

namespace LinqToSolr.Models
{
    public class LinqToSolrFacet<TResult> : ILinqToSolrFacet<TResult>
    {
        public IEnumerable<TResult> Documents => responce.Response.Documents;
        readonly SolrResponse<TResult> responce;
        public LinqToSolrFacet(SolrResponse<TResult> responce)
        {
            this.responce = responce;
        }

        public IDictionary<TKey, int> Get<TKey>(Expression<Func<TResult, TKey>> prop)
        {
            return responce.Facets.Get(prop);
        }
    }

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