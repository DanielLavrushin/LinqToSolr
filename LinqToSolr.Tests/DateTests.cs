using LinqToSolr.Extensions;
using LinqToSolr.Tests.Models;

namespace LinqToSolr.Tests
{
    [TestClass]
    public class DateTests : BaseTest
    {

        [TestMethod("Date rangle test")]
        public async Task DateRangeTest()
        {
            var start = new DateTime(2018, 1, 1);
            var end = new DateTime(2020, 12, 31);
            var docs = await Service.AsQueryable<SolrDocument>().Where(x => x.Registered >= start && x.Registered <= end).ToListAsync();
            Assert.IsNotNull(docs, "The result should not be null");
            Assert.IsTrue(docs.Count > 0, "There should be at least one document");
            Assert.IsTrue(docs.All(x => x.Registered >= start && x.Registered <= end), $"All documents should have Date between {start} and {end}");
        }
    }
}
