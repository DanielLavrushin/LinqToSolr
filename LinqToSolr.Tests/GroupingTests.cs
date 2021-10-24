using System;
using System.Linq;

using NUnit.Framework;

namespace LinqToSolr.Tests
{
    [TestFixture]
    public class GroupingTests : BaseFixture
    {
        [Test]
        public void GroupByTest()
        {
            var docsNum = 100;
            factory.DeleteAll();
            var docs = factory.GenerateDocs(docsNum);
            var parent1 = Guid.NewGuid();
            var parent2 = Guid.NewGuid();
            foreach (var d in docs.Take(50))
                d.ParentId = parent1;
            foreach (var d in docs.Skip(50))
                d.ParentId = parent2;

            factory.AddOrUpdate(docs);

            var groups = factory.Queriable().Take(docsNum).GroupBy(x => x.ParentId).ToList();

            Assert.NotNull(groups);
            Assert.True(groups.Count == 2);
            Assert.True(groups.First().Key == parent1 && groups.First().Count() == 50);
            Assert.True(groups.Last().Key == parent2 && groups.Last().Count() == 50);

        }


    }
}
