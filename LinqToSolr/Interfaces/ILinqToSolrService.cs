using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Linq;
using LinqToSolr.Query;
using LinqToSolr.Interfaces;

namespace LinqToSolr.Services
{
    public interface ILinqToSolrService : IDisposable
    {
        Uri LastResponseUrl { get; set; }
        ILinqToSolrConfiguration Configuration { get; }
        ILinqToSolrProvider Provider { get; }
        IQueryable<TResult> AsQueryable<TResult>();


        void DeleteAll<TObject>(bool softCommit = false);
        IEnumerable<TResult> AddOrUpdate<TResult>(IEnumerable<TResult> document, bool softCommit = false);

    }
}