using LinqToSolr.Extensions;
using LinqToSolr.Tests.Models;

namespace LinqToSolr.Tests
{
    [TestClass]
    public class DateTests
    {
        LinqToSolrService service;
        ICollection<SolrDocument> localDocuments;
        public DateTests()
        {
            var config = new LinqToSolrConfiguration(new LinkToSolrEndpoint("http://localhost:8983/solr")).MapCoreFor<SolrDocument>("dummy");
            service = new LinqToSolrService(config);
            localDocuments = JsonParser.FromJson<List<SolrDocument>>(File.ReadAllText("dummy-data.json"));
        }

        [TestMethod]
        public async Task DateRangeTest()
        {
            var start = new DateTime(2018, 1, 1);
            var end = new DateTime(2020, 12, 31);
            var docs = await service.AsQueryable<SolrDocument>().Where(x => x.Registered >= start && x.Registered <= end).ToListAsync();
            Assert.IsNotNull(docs, "The result should not be null");
            Assert.IsTrue(docs.Count > 0, "There should be at least one document");
            Assert.IsTrue(docs.All(x => x.Registered >= start && x.Registered <= end), $"All documents should have Date between {start} and {end}");
        }
    }
}
