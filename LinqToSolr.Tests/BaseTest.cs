﻿using LinqToSolr.Tests.Models;
using System.Diagnostics;

namespace LinqToSolr.Tests
{
    [TestClass]
    public abstract class BaseTest
    {
        public TestContext TestContext { get; set; }
        internal ILinqToSolrService Service;
        public static Guid DocId;
        public string solrUrl;
        public string solrCore;
        public string solrUser;
        public string solrPassword;
        public ILinqToSolrQueriable<SolrDocument> Query;

        [TestInitialize]
        public void Initialize()
        {
            solrUrl = TestContext.Properties["solrUrl"]?.ToString();
            solrCore = TestContext.Properties["solrCore"]?.ToString();
            solrUser = TestContext.Properties["solrUser"]?.ToString();
            solrPassword = TestContext.Properties["solrPassword"]?.ToString();
            var config = new LinqToSolrConfiguration(new LinqToSolrEndpoint(solrUrl, solrUser, solrPassword)).MapCoreFor<SolrDocument>(solrCore);
            Service = new LinqToSolrService(config);
            Query = Service.AsQueryable<SolrDocument>() as ILinqToSolrQueriable<SolrDocument>;
        }

 
    }
}
