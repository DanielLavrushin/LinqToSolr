using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using LinqToSolr.Data;

namespace LinqToSolr.Services
{
    public interface ILinqToSolrService
    {
        ILinqToSolrResponse LastResponse { get; set; }
        Type ElementType { get; set; }
        LinqToSolrRequestConfiguration Configuration { get; set; }
        object Query(Type type, LinqToSolrQuery query = null);
        ICollection<T> Query<T>(LinqToSolrQuery query = null);

        LinqToSolrQuery CurrentQuery { get; set; }


        void AddOrUpdate<T>(params T[] document);
        void Delete<T>(params object[] documentId);
        void Delete<T>(Expression<Func<T, bool>> query);
    }
}