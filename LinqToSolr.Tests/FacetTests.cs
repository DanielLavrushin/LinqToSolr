using LinqToSolr.Extensions;
using LinqToSolr.Tests.Models;

namespace LinqToSolr.Tests
{
    [TestClass]
    public class FacetTests : BaseTest
    {
        [TestMethod]
        public async Task FacetSelectTest()
        {
            var docs1 = await Service.AsQueryable<SolrDocument>().GroupByFacets(x => x.Age).ToListAsync();
            Assert.IsNotNull(docs1, "The result should not be null");
            Assert.IsTrue(docs1.Count > 0, "There should be at least one document");
            Assert.IsTrue(docs1.Count == 2, "There should be two groups");
        }
    }
}
