
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using LinqToSolr.Services;
using LinqToSolr.Tests.Models;

using NUnit.Framework;

namespace LinqToSolr.Tests
{
    [TestFixture]
    public class AddOrUpdateTests : BaseFixture
    {


        [Test]
        public async Task AddDocuments()
        {
            var docsNum = 1000;
            var docs = factory.GenerateDocs(docsNum);
            var parentId = docs.First().ParentId;
            Log($"parent id: {parentId}");

            await factory.AddOrUpdate(docs.ToArray());

            var result = factory.Limit(docsNum).Query(x => x.ParentId == parentId);

            Assert.NotNull(result);
            Assert.IsTrue(docsNum == result.Count(), $"expected {docsNum} docs, but got {result.Count()}");

        }

        [Test]
        public void UpdateDocuments()
        {
            var docsNum = 100;
            var docs = factory.GenerateDocs(docsNum);
            var parentId = docs.First().ParentId;
            var parentIdNew = Guid.NewGuid();
            Log($"parent id: {parentId}");
            Log($"new parent id: {parentIdNew}");

            factory.AddOrUpdate(docs.ToArray());

            foreach (var d in docs.Skip(50))
            {
                d.ParentId = parentIdNew;
            }

            factory.AddOrUpdate(docs.ToArray());

            var result = factory.Reset().Limit(docsNum).Query(x => x.ParentId == parentId || x.ParentId == parentIdNew);


            Assert.NotNull(result);

            var olddocsCount = result.Count(x => x.ParentId == parentId);
            var newdocsCount = result.Count(x => x.ParentId == parentIdNew);
            Assert.IsTrue(olddocsCount == 50, $"expected 50, got {olddocsCount}");
            Assert.IsTrue(newdocsCount == 50, $"expected 50, got {newdocsCount}");

        }


    }
}
