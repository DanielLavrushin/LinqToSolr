using LinqToSolr.Extensions;
using LinqToSolr.Tests.Models;

namespace LinqToSolr.Tests
{
    [TestClass]
    public class FacetTests : BaseTest
    {
        [TestMethod("Select facets test")]
        public async Task FacetSelectTest()
        {
            var docs = await Query.ToFacetsAsync(x => x.IsActive, x => x.Age) as LinqToSolrFacetDictionary<SolrDocument>;
            var activelist = docs.GetFacet(x => x.IsActive);
            Assert.IsNotNull(docs, "The result should not be null");
            Assert.IsTrue(docs.Count > 0, "There should be at least one document");
            Assert.IsTrue(docs.Count == 2, "There should be two groups");

        }

        [TestMethod("Select facets multifield test")]
        public async Task FacetMultiFieldTest()
        {
            var docs = await Query.ToFacetsAsync(x => x.Tags) as LinqToSolrFacetDictionary<SolrDocument>;
            var activelist = docs.GetFacet<string[], string>(x => x.Tags);
            Assert.IsNotNull(docs, "The result should not be null");

        }
    }
}
