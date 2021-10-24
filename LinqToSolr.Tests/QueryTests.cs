using System;
using System.Collections.Generic;
using System.Linq;

using LinqToSolr.Data;
using LinqToSolr.Services;
using LinqToSolr.Tests.Models;

using NUnit.Framework;


namespace LinqToSolr.Tests
{



    [TestFixture]
    public class QueryTests : BaseFixture
    {

        [Test]
        public void FirstOrDefaultTest()
        {
            var expected = factory.GenerateDocs(1).First();
            factory.AddOrUpdate(expected);

            var doc = factory.Reset().Limit(999).Query(x => x.Id == expected.Id).FirstOrDefault();

            Assert.IsNotNull(doc);
            Assert.IsTrue(doc.Id == expected.Id, $"expected: {expected.Id}, got : {doc.Id}");
        }


        [Test]
        public void WhereTest()
        {
            var docsNum = 10;
            var expected = factory.GenerateDocs(docsNum);
            var parentId = expected.First().ParentId;
            factory.AddOrUpdate(expected);

            var doc = factory.Reset().Limit(docsNum).Query(x => x.ParentId == parentId);

            Assert.IsNotNull(doc);
            Assert.IsTrue(doc.Count() == docsNum, $"expected: {docsNum}, got : {doc.Count() }");
        }

        [Test]
        public void ContainsTest()
        {
            var docsNum = 10;
            var expected = factory.GenerateDocs(docsNum);
            factory.AddOrUpdate(expected);

            var doc = factory.Reset().Limit(docsNum).Query(x => x.Name.Contains("CoreDoc"));

            Assert.IsNotNull(doc);
            Assert.IsTrue(doc.Any());
            Assert.IsTrue(doc.First().Name.Contains("CoreDoc"));
        }

        [Test]
        public void QueryAndTest()
        {

            var expected = factory.GenerateDoc();
            factory.AddOrUpdate(expected);

            var doc = factory.Reset().Query(x => x.Id == expected.Id && x.ParentId == expected.ParentId);

            Assert.IsNotNull(doc);
            Assert.IsTrue(doc.Count() == 1);
            Assert.IsTrue(doc.First().Id == expected.Id);
        }

        [Test]
        public void QueryOrTest()
        {

            var docs = factory.GenerateDocs(2);
            factory.AddOrUpdate(docs);

            var result = factory.Reset().Query(x => x.Id == docs.First().Id || x.Id == docs.Last().Id);

            Assert.IsNotNull(result);
            Assert.IsTrue(result.Count() == 2);
            Assert.IsTrue(result.First().Id == docs.First().Id);
            Assert.IsTrue(result.Last().Id == docs.Last().Id);
        }

        [Test]
        public void StartWithTest()
        {
            var docsNum = 10;
            var expected = factory.GenerateDocs(docsNum);
            factory.AddOrUpdate(expected);

            var doc = factory.Reset().Limit(docsNum).Query(x => x.Name.StartsWith("TestCoreDoc"));

            Assert.IsNotNull(doc);
            Assert.IsTrue(doc.Any());
            Assert.IsTrue(doc.First().Name.StartsWith("TestCoreDoc"));
        }

        [Test]
        public void EndsWithTest()
        {
            var docsNum = 100;
            var pattern = "99";
            var expected = factory.GenerateDocs(docsNum);
            factory.AddOrUpdate(expected);

            var doc = factory.Reset().Limit(docsNum).Query(x => x.Name.EndsWith(pattern));

            Assert.IsNotNull(doc);
            Assert.IsTrue(doc.Any());
            Assert.IsTrue(doc.First().Name.EndsWith(pattern));
        }

        [Test]
        public void ArrayContainsTest()
        {
            var docsNum = 10;
            var expected = factory.GenerateDocs(docsNum);
            var parentId = expected.First().ParentId;
            factory.AddOrUpdate(expected);
            var cities = factory.Cities.Take(4).ToArray();
            var doc = factory.Reset().Limit(docsNum).Query(x => cities.Contains(x.City));

            Assert.IsNotNull(doc);
            Assert.IsTrue(doc.Count() == docsNum, $"expected: {docsNum}, got : {doc.Count() }");
        }

        [Test]
        public void MultiValueFieldTest()
        {
            string pattern = "Copenhagen";
            var docsNum = 10;
            var expected = factory.GenerateDocs(docsNum);
            factory.AddOrUpdate(expected);

            var docs = factory.Reset().Limit(docsNum).Query(x => x.Sites.Contains(pattern));

            Assert.IsNotNull(docs);
            Assert.IsTrue(docs.Any());
            Assert.IsTrue(docs.All(x => x.Sites.Contains(pattern)));
        }

        [Test]
        public void QueryByTimeTest()
        {
            var docsNum = 10;
            var doc = factory.GenerateDoc();
            factory.AddOrUpdate(doc);

            var result = factory.Reset().Limit(docsNum).Query(x => x.Time == doc.Time);

            Assert.IsNotNull(result);
            Assert.IsTrue(result.Any());
            Assert.IsTrue(doc.Time.ToLongDateString() == result.First().Time.ToLongDateString(), $"expected {doc.Time.ToLongDateString()}, got : {result.First().Time.ToLongDateString()}");
        }

        [Test]
        public void CompareDatesTest()
        {
            var docsNum = 10;
            var doc = factory.GenerateDoc();
            doc.Time = new DateTime(1984, 09, 16, 0, 0, 0);
            factory.AddOrUpdate(doc);

            var result = factory.Reset().Limit(docsNum).Query(x => x.Time >= doc.Time.AddDays(-2));

            Assert.IsNotNull(result);
            Assert.IsTrue(result.Any(), "no results found ");
        }
    }
}
