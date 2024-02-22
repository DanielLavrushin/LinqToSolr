using LinqToSolr.Tests.Models;
using System.Diagnostics;

namespace LinqToSolr.Tests
{
    [TestClass]
    public class ConnectionTests
    {
        [TestMethod]
        public void InitServer()
        {
            var guidstr = "f1e1865f-46c7-4398-8bb6-3c1f6638b1d0";
            var guid = new Guid(guidstr);
            var date = DateTime.Now;
            var doc = new SolrDocument
            {
                Id = guid,
                Guid = guid,
                Index = 0,
                IsActive = true,
                Name = "dges Pit",
            };

            var config = new LinqToSolrConfiguration(new LinkToSolrEndpoint("http://localhost:8983/solr"))
                .MapCoreFor<SolrDocument>("dummy");

            var service = new LinqToSolrService(config);
            var list = service.AsQueryable<SolrDocument>().Where(x => x.Index > 5 && x.Index <= 10).FirstOrDefault();
        }

        [TestMethod]
        public void TestSolrUrlParser()
        {
            string[] uris = {
            "http://localhost:8983",
            "http://localhost:8983/",
            "http://localhost:8983/solr",
            "http://localhost:8983/solr/",
            "http://localhost:8983/solr/asdasd/#/",
            "http://localhost:8983/customesolr/",
            "http://localhost:8983/customsolr/solr"
        };
            foreach (var uri in uris)
            {
                var endpoint = new LinkToSolrEndpoint(uri);
                Debug.WriteLine(endpoint.SolrUri);
            }
        }
    }
}