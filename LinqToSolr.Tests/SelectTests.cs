using LinqToSolr.Extensions;
using LinqToSolr.Tests.Models;

namespace LinqToSolr.Tests
{
    [TestClass]
    public class SelectTests : BaseTest
    {
        [TestMethod]
        public async Task SelectSimpleDynamicToListTest()
        {
            var docs1 = await Service.AsQueryable<SolrDocument>().Select(x => x.Name).ToListAsync();
            var docs2 = await Service.AsQueryable<SolrDocument>().Select(x => new { x.Index, x.Name }).ToListAsync();

            Assert.IsNotNull(docs1, "The result should not be null");
            Assert.IsNotNull(docs2, "The result should not be null");
            Assert.IsTrue(docs1.Count > 0, "There should be at least one document");
            Assert.IsTrue(docs2.Count > 0, "There should be at least one document");
        }

        [TestMethod]
        public async Task SelectWithWhereToStringListTest()
        {
            var docs1 = await Service.AsQueryable<SolrDocument>().Take(50).Where(x => x.Name.StartsWith("A")).Select(x => x.Name).ToListAsync();

            Assert.IsNotNull(docs1, "The result should not be null");
            Assert.IsTrue(docs1.Count > 0, "There should be at least one document");
            Assert.IsFalse(docs1.Any(x => string.IsNullOrWhiteSpace(x)), "There should not be any whitespace names");
            Assert.IsTrue(docs1.All(x => x.StartsWith("A")), "All names should start with 'A'");
        }

    }
}
