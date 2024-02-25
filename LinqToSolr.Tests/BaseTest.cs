using LinqToSolr.Tests.Models;

namespace LinqToSolr.Tests
{
    [TestClass]
    public abstract class BaseTest
    {
        public TestContext TestContext { get; set; }
        internal ICollection<SolrDocument> localDocuments;
        internal LinqToSolrService Service;

        public string SolrUrl;
        public string SolrCore;
        public string SolrUser;
        public string SolrPassword;

        [TestInitialize]
        public void Initialize()
        {
            SolrUrl = TestContext.Properties["solrUrl"]?.ToString();
            SolrCore = TestContext.Properties["solrCore"]?.ToString();
            SolrUser = TestContext.Properties["solrUser"]?.ToString();
            SolrPassword = TestContext.Properties["solrPassword"]?.ToString();
            var config = new LinqToSolrConfiguration(new LinkToSolrEndpoint(SolrUrl)).MapCoreFor<SolrDocument>(SolrCore);
            Service = new LinqToSolrService(config);
        }
    }
}
