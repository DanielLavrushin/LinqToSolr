﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using LinqToSolr.Services;
using NUnit.Framework;
using LinqToSolr.Tests.Models;
using LinqToSolr.Models;

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
            var config = new LinqToSolrConfiguration("http://192.168.1.99:81/solr")
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
