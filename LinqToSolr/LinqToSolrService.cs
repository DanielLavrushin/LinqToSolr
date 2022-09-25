using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Net;
using System.Reflection;

using LinqToSolr.Query;
using LinqToSolr.Models;
using LinqToSolr.Helpers.Json;
using LinqToSolr.Interfaces;
using System.Threading.Tasks;

namespace LinqToSolr.Services
{
    public class LinqToSolrService : ILinqToSolrService
    {
        public ILinqToSolrProvider Provider { get; }
        public ILinqToSolrConfiguration Configuration { get; }
        public Uri LastResponseUrl { get; set; }
        public LinqToSolrService() : this(LinqToSolrConfiguration._instance)
        {
        }

        public LinqToSolrService(ILinqToSolrConfiguration configuration)
        {
            Configuration = configuration;
            if (Configuration.Take <= 0)
            {
                Configuration.Take = 10;
            }
            if (Configuration.EndPoint == null)
                throw new Exception("Solr Endpoint was not provided. Specify Solr server in SolrRequestConfiguration.");

            Provider = new LinqToSolrProvider(this);
        }

        public IQueryable<T> AsQueryable<T>()
        {
            return new LinqToSolrQueriable<T>(Provider, null);
        }
        public async Task<IEnumerable<TObject>> AddOrUpdate<TObject>(params TObject[] documents)
        {
            return await AddOrUpdate(false, documents);
        }
        public async Task<IEnumerable<TObject>> AddOrUpdate<TObject>(bool softCommit = false, params TObject[] document)
        {
            if (document == null || !document.Any())
                throw new ArgumentNullException(nameof(document));

            return await Provider.AddOrUpdate(softCommit, document);
        }

        public async Task DeleteAll<TObject>(bool softCommit = false)
        {
            await Provider.DeleteAll<TObject>();
        }

        public void Dispose()
        {

        }
    }
}
