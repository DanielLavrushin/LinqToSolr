
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

using LinqToSolr.Data;
using LinqToSolr.Query;
using LinqToSolr.Services;
using LinqToSolr.Tests.Models;

using NUnit.Framework;

namespace LinqToSolr.Tests
{
    [TestFixture]
    public class DeleteTests : BaseFixture
    {
        [Test]
        public void DeleteAllDocuments()
        {
            var docsNum = 100;
            var docs = factory.GenerateDocs(docsNum);
            var parentId = docs.First().ParentId;
            factory.AddOrUpdate(docs);

            var before = factory.Reset().Limit(docsNum).Query(x => x.ParentId == parentId);
            Assert.IsTrue(before.Count() == docsNum, $"found different number of docs. Exprected {docsNum}, found : {before.Count()}");

            factory.Reset().DeleteAll();

            var after = factory.Reset().Limit(docsNum).Query(x => x.ParentId == parentId);
            Assert.IsTrue(after.Count() == 0);
            Assert.IsTrue(after.Count() == 0, "found different number of docs, found : {after.Count()}");
        }

        [Test]
        public void DeleteByQuery()
        {
            var docsNum = 3;
            var docs = factory.GenerateDocs(docsNum);
            factory.AddOrUpdate(docs);

            var docIds = docs.Select(x => x.Id).ToArray();
            Expression<Func<TestCoreDoc, bool>> query = x => docIds.Contains(x.Id);

            foreach (var did in docIds)
                Console.WriteLine($"did : {did}");

            factory.Delete(query);

            var after = factory.Query(query);
            Assert.NotNull(after);
            Assert.IsTrue(after.Count() == 0, $"expected 0 result after deleing documents.Found {after.Count()}");
        }

        [Test]
        public void DeleteByIds()
        {
            var docsNum = 3;
            var docs = factory.GenerateDocs(docsNum);
            factory.AddOrUpdate(docs);

            var docIds = docs.Select(x => x.Id).ToArray();
            Expression<Func<TestCoreDoc, bool>> query = x => docIds.Contains(x.Id);

            foreach (var did in docIds)
                Console.WriteLine($"did : {did}");

            factory.Delete(docIds);

            var after = factory.Query(query);
            Assert.NotNull(after);
            Assert.IsTrue(after.Count() == 0, $"expected 0 result after deleing documents.Found {after.Count()}");
        }
    }
}
