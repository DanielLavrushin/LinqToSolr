using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using LinqToSolr.Services;
using LinqToSolr.Query;
using LinqToSolr.Data;
using LinqToSolr.Interfaces;
using System.Linq.Expressions;
using NUnit.Framework;
using LinqToSolr.Tests.Models;

namespace LinqToSolr.Tests
{
    [TestFixture]
    public class BaseFixture
    {
        public SolrServiceFactory factory;
        internal ILinqToSolrService solr;
        [OneTimeSetUp]
        public virtual void OneTimeSetUp()
        {
            var config = new LinqToSolrConfiguration("https://localhost:9193/solr")
                .MapIndexFor<TestCoreDoc>("testcore");
            solr = new LinqToSolrService(config);
            factory = new SolrServiceFactory(solr);
        }

        [OneTimeTearDown]
        public virtual void OneTimeTearDown()
        {
        }
        [TearDown]
        public virtual void TearDown()
        {
            Console.WriteLine(factory.solr.LastResponseUrl.ToString());
        }
        public void Log(string message)
        {
            Console.WriteLine(message);
        }
    }
}
