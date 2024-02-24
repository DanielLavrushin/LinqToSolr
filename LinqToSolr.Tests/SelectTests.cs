using LinqToSolr.Extensions;
using LinqToSolr.Tests.Models;

namespace LinqToSolr.Tests
{
    [TestClass]
    public class SelectTests
    {
        LinqToSolrService service;
        ICollection<SolrDocument> localDocuments;
        public SelectTests()
        {
            var config = new LinqToSolrConfiguration(new LinkToSolrEndpoint("http://localhost:8983/solr")).MapCoreFor<SolrDocument>("dummy");
            service = new LinqToSolrService(config);
       //     localDocuments = JsonParser.FromJson<List<SolrDocument>>(File.ReadAllText("dummy-data.json"));
        }

        [TestMethod]
        public async Task SimpleSelectDynamicTest()
        {
            var docs2 = await service.AsQueryable<SolrDocument>().Select(x => new { x.Index, x.Name }).ToListAsync();
            var docs1 = await service.AsQueryable<SolrDocument>().Select(x => x.Name).ToListAsync();
        }

    }
}
