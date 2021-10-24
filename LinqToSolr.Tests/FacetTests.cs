﻿using System;

using NUnit.Framework;

namespace LinqToSolr.Tests
{
    [TestFixture]
    public class FacetTests : BaseFixture
    {
        [Test]
        public void SimpleFacetsTest()
        {
            factory.DeleteAll();
            var docs = factory.GenerateDocs(100);
            factory.AddOrUpdate(docs);

            var facets = factory.Queriable().ToFacets(x => x.City, x => x.Name);

            var cityGroup = facets.Get(x => x.City);
            foreach (var kv in cityGroup)
            {
                Console.WriteLine("{0}: {1}", kv.Key, kv.Value);
            }

            Assert.IsTrue(facets.Get(x => x.City).Count > 0);
            Assert.IsTrue(facets.Get(x => x.Name).Count > 0);
        }

        [Test]
        public void ExcludeFacetsTest()
        {
            factory.DeleteAll();
            var docs = factory.GenerateDocs(100);
            factory.AddOrUpdate(docs);

            var facets = factory.Queriable().ExcludeFacets(x => x.Name).ToFacets(x => x.City, x => x.Name);

            var cityGroup = facets.Get(x => x.City);
            foreach (var kv in cityGroup)
            {
                Console.WriteLine("{0}: {1}", kv.Key, kv.Value);
            }

            Assert.IsTrue(facets.Get(x => x.City).Count > 0);
            Assert.IsTrue(facets.Get(x => x.Name).Count > 0);
        }
    }
}