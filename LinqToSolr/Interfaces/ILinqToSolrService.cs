using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Linq;
using LinqToSolr.Data;
using LinqToSolr.Query;
using LinqToSolr.Interfaces;

namespace LinqToSolr.Services
{
    public interface ILinqToSolrService
    {
        Uri LastResponseUrl { get; set; }
        ILinqToSolrConfiguration Configuration { get; }
        ILinqToSolrProvider Provider { get; }
        IQueryable<T> AsQueryable<T>();

        //   ICollection<T> LastDocuments<T>();
        //    void AddOrUpdate<T>(T[] document, bool softCommit = false);
        //     void AddOrUpdate<T>(T document, bool softCommit = false);
        //      void Delete<T>(object[] documentId, bool softCommit = false);
        //      void Delete<T>(object documentId, bool softCommit = false);
        //  void Delete<T>(Expression<Func<T, bool>> query, bool softCommit = false);
    }
}