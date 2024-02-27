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
            var docs = await Query.ToFacetsAsync(x => x.IsActive, x => x.Age);
            var activelist = docs[x => x.IsActive].Cast<bool>();
            var ageslist = docs[x => x.Age].Cast<int>();
            Assert.IsNotNull(docs, "The result should not be null");
            Assert.IsTrue(docs.Count > 0, "There should be at least one document");
            Assert.IsTrue(docs.Count == 2, "There should be two groups");
        }
    }
}
