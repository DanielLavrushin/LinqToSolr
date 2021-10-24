
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

using LinqToSolr.Tests.Models;

using NUnit.Framework;

namespace LinqToSolr.Tests
{
    [TestFixture]
    public class SelectTests : BaseFixture
    {
        public class TempAnotherClass
        {
            [SolrField("id")]
            public Guid Id { get; set; }
            [SolrField("name")]
            public string Name { get; set; }
            [SolrField("city")]
            public string City { get; set; }
        }

        [Test]
        public void SelectSpecificFieldTest()
        {
            var docsNum = 100;
            string pattern = "Copenhagen";
            var docs = factory.GenerateDocs(docsNum);
            var parentId = docs.First().ParentId;
            factory.AddOrUpdate(docs);

            var result = factory.Queriable().Where(x => x.ParentId == parentId).Take(docsNum).Where(x => x.City == pattern).Select(x => x.City).ToList();

            Assert.NotNull(result);
            Assert.True(result.All(x => x == pattern));

        }

        [Test]
        public void SelectDynamicObjectTest()
        {
            var docsNum = 100;
            string pattern = "Copenhagen";
            var docs = factory.GenerateDocs(docsNum);
            var parentId = docs.First().ParentId;
            factory.AddOrUpdate(docs);

            var result = factory.Queriable().Where(x => x.ParentId == parentId).Take(docsNum).Where(x => x.City == pattern).Select(x => new { x.City, x.Name, x.Id }).ToList();

            Assert.NotNull(result);
            Assert.True(result.All(x => x.City == pattern));
            Assert.True(result.All(x => x.Id != Guid.Empty));
            Assert.True(result.All(x => x.Name.StartsWith(nameof(TestCoreDoc))));
        }

        [Test]
        public void SelectOtherClassTest()
        {
            var docsNum = 100;
            string pattern = "Copenhagen";
            var docs = factory.GenerateDocs(docsNum);
            var parentId = docs.First().ParentId;
            factory.AddOrUpdate(docs);

            var result = factory.Queriable().Where(x => x.ParentId == parentId).Take(docsNum).Where(x => x.City == pattern).Select(x => new TempAnotherClass { City = x.City, Name = x.Name, Id = x.Id }).ToList();

            Assert.NotNull(result);
            Assert.True(result.All(x => x.City == pattern));
            Assert.True(result.All(x => x.Id != Guid.Empty));
        }
    }
}
