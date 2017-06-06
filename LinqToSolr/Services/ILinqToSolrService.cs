using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using LinqToSolr.Data;
using LinqToSolr.Query;

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

        LinqToSolrQueriable<T> AsQueryable<T>();

        void AddOrUpdate<T>(T[] document, bool softCommit = false);
        void AddOrUpdate<T>(T document, bool softCommit = false);
        void Delete<T>(object[] documentId, bool softCommit = false);
        void Delete<T>(object documentId, bool softCommit = false);
        void Delete<T>(Expression<Func<T, bool>> query, bool softCommit = false);
    }
}