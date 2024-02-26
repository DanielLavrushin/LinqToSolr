using LinqToSolr.Extensions;
using LinqToSolr.Tests.Models;

namespace LinqToSolr.Tests
{
    [TestClass]
    public class PostTests : BaseTest
    {
        [TestMethod]
        public async Task SelectSimpleDynamicToListTest()
        {
            var docs1 = await Service.AsQueryable<SolrDocument>().AsPostMethod().Select(x => x.Name).ToListAsync();

            Assert.IsNotNull(docs1, "The result should not be null");
            Assert.IsTrue(docs1.Count > 0, "There should be at least one document");
        }
    }
}
