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
    public class SortAndOrderTests : BaseFixture
    {
        [Test]
        public void OrderAscTest()
        {
            factory.DeleteAll();
            var docs = factory.GenerateDocs(10);
            var date = new DateTime(1984, 1, 1);
            docs.Last().Time = date;

            factory.AddOrUpdate(docs);

            var result = factory.Reset().Queriable().OrderBy(x => x.Time).ToList();

            Assert.IsNotNull(result);
            Assert.IsTrue(result.Any());
            Assert.IsTrue(result.First().Time.Year == date.Year, $"expected: {date.Year}, got : {result.First().Time.Year}");
        }

        [Test]
        public void OrderDescTest()
        {
            factory.DeleteAll();
            var docs = factory.GenerateDocs(10);
            var date = new DateTime(2050, 1, 1);
            docs.Last().Time = date;

            factory.AddOrUpdate(docs);

            var result = factory.Reset().Queriable().OrderByDescending(x => x.Time).ToList();

            Assert.IsNotNull(result);
            Assert.IsTrue(result.Any());
            Assert.IsTrue(result.First().Time.Year == date.Year, $"expected: {date.Year}, got : {result.First().Time.Year}");
        }


        [Test]
        public void SkipTakleTest()
        {
            factory.DeleteAll();
            var take = 100;
            var skip = 50;
            var docs = factory.GenerateDocs(500);
            factory.AddOrUpdate(docs);

            var result = factory.Reset().Queriable().Skip(skip).Take(take).ToList();

            Assert.IsNotNull(result);
            Assert.IsTrue(result.Any());
            Assert.IsTrue(result.Count() == take);
            Assert.IsTrue(result.First().Name == docs.Skip(skip).Take(take).First().Name, "expected: {0} doc name is {1}", docs.Skip(skip).First().Name, result.First().Name);
            Assert.IsTrue(result.Last().Name == docs.Skip(skip).Take(take).Last().Name, "expected: {0} doc name is {1}", docs.Skip(skip).Last().Name, result.Last().Name);
        }
    }
}
