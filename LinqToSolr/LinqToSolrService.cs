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

        public IEnumerable<TObject> AddOrUpdate<TObject>(IEnumerable<TObject> documents, bool softCommit = false)
        {
            if (documents == null)
                throw new ArgumentNullException(nameof(documents));

            return Provider.AddOrUpdate(documents, softCommit);
        }

        public void DeleteAll<TObject>(bool softCommit = false)
        {
            Provider.DeleteAll<TObject>();
        }

        public void Dispose()
        {

        }
    }
}
