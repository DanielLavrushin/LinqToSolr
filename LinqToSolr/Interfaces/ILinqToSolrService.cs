using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Linq;
using LinqToSolr.Query;
using LinqToSolr.Interfaces;
using System.Threading.Tasks;

namespace LinqToSolr.Services
{
    public interface ILinqToSolrService : IDisposable
    {
        Uri LastResponseUrl { get; set; }
        ILinqToSolrConfiguration Configuration { get; }
        ILinqToSolrProvider Provider { get; }
        IQueryable<TResult> AsQueryable<TResult>();


        Task DeleteAll<TObject>(bool softCommit = false);
        Task<IEnumerable<TResult>> AddOrUpdate<TResult>(params TResult[] document);
        Task<IEnumerable<TResult>> AddOrUpdate<TResult>(bool softCommit = false, params TResult[] document);

    }
}