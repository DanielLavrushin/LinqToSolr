using LinqToSolr.Tests.Models;
using System.Diagnostics;

namespace LinqToSolr.Tests
{
    [TestClass]
    public class WhereTests
    {
        LinqToSolrService service;
        public WhereTests()
        {
            var config = new LinqToSolrConfiguration(new LinkToSolrEndpoint("http://localhost:8983/solr")).MapCoreFor<SolrDocument>("dummy");

            service = new LinqToSolrService(config);
        }
        [TestMethod]
        public void EqualInt()
        {
            var doc = service.AsQueryable<SolrDocument>().Where(x => x.Index == 1).FirstOrDefault();
            Assert.IsNotNull(doc, "The result should not be null");
            Assert.AreEqual(1, doc.Index, "The index should be 1");
        }
        [TestMethod]
        public void EqualString()
        {
            var company = "TALKOLA";
            var doc = service.AsQueryable<SolrDocument>().Where(x => x.Company == company).FirstOrDefault();
            Assert.IsNotNull(doc, "The result should not be null");
            Assert.AreEqual(company, doc.Company, "The company should be TALKOLA");

        }
        [TestMethod]
        public void GreaterInteger()
        {
            var docs = service.AsQueryable<SolrDocument>().Where(x => x.Balance >= 1000 && x.Balance < 2000).ToList();
            Assert.IsNotNull(docs, "The result should not be null");
            Assert.IsTrue(docs.Count > 0, "There should be at least one document");
            Assert.IsTrue(docs.All(x => x.Balance >= 1000 && x.Balance < 2000), "All documents should have Balance >= 1000 and balance < 2000");
        }

        [TestMethod]
        public void ContainsInMultilist()
        {
            var docs = service.AsQueryable<SolrDocument>().Where(x => x.Tags.Contains("Denmark")).ToList();
            Assert.IsNotNull(docs, "The result should not be null");
            Assert.IsTrue(docs.Count > 0, "There should be at least one document");
            Assert.IsTrue(docs.All(x => x.Tags.Contains("Denmark")), "All documents should have Denmark in the tags");
        }
        [TestMethod]
        public void ContainsInString()
        {
            var docs = service.AsQueryable<SolrDocument>().Where(x => x.Address.Contains("Nassau")).ToList();
            Assert.IsNotNull(docs, "The result should not be null");
            Assert.IsTrue(docs.Count > 0, "There should be at least one document");
            Assert.IsTrue(docs.All(x => x.Address.Contains("Nassau")), "All documents should have Nassau in the address");
        }
        [TestMethod]
        public void ContainsInArray()
        {
            var array = new[] { 1, 5, 10 };
            var docs = service.AsQueryable<SolrDocument>().Where(x => array.Contains(x.Index)).ToList();
            Assert.IsNotNull(docs, "The result should not be null");
            Assert.IsTrue(docs.Count > 0, "There should be at least one document");
            Assert.IsTrue(docs.All(x => array.Contains(x.Index)), "All documents should have Index in the array");
        }
    }
}