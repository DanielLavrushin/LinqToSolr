using LinqToSolr.Tests.Models;

namespace LinqToSolr.Tests
{
    [TestClass]
    public class ConnectionTests
    {
        [TestMethod]
        public void InitServer()
        {
            var guidstr = "00000000-0000-0000-0000-000000000000";
            var guid = new Guid(guidstr);
            var date = DateTime.Now;

            var config = new LinqToSolrConfiguration(new LinkToSolrEndpoint("http://localhost:8983/solr/"));

            var service = new LinqToSolrService(config);
            var list = service.AsQueryable<SolrDocument>().Where(x => x.IsActive || x.IsActive == false).ToList();
        }
    }
}