using LinqToSolr.Extensions;
using LinqToSolr.Tests.Models;

namespace LinqToSolr.Tests
{
    [TestClass]
    public class GroupByTests : BaseTest
    {
        [TestMethod]
        public async Task GroupByTest()
        {
            var docs1 = await Service.AsQueryable<SolrDocument>().GroupBy(x => x.IsActive).ToListAsync();
            Assert.IsNotNull(docs1, "The result should not be null");
            Assert.IsTrue(docs1.Count > 0, "There should be at least one document");
            Assert.IsTrue(docs1.Count == 2, "There should be two groups");
        }
    }
}
