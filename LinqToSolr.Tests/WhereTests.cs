using LinqToSolr.Extensions;
using LinqToSolr.Tests.Models;

namespace LinqToSolr.Tests
{
    [TestClass]
    public class WhereTests
    {
        LinqToSolrService service;
        ICollection<SolrDocument> localDocuments;
        public WhereTests()
        {
            var config = new LinqToSolrConfiguration(new LinkToSolrEndpoint("http://localhost:8983/solr")).MapCoreFor<SolrDocument>("dummy");
            service = new LinqToSolrService(config);
            localDocuments = JsonParser.FromJson<List<SolrDocument>>(File.ReadAllText("dummy-data.json"));
        }

        [TestMethod]
        public async Task EqualInt()
        {
            var doc = await service.AsQueryable<SolrDocument>().Where(x => x.Index == 1).FirstOrDefaultAsync();
            Assert.IsNotNull(doc, "The result should not be null");
            Assert.AreEqual(1, doc.Index, "The index should be 1");
        }

        [TestMethod]
        public async Task EqualString()
        {
            var company = "DIGIQUE";
            var doc = await service.AsQueryable<SolrDocument>().Where(x => x.Company == company).FirstOrDefaultAsync();
            Assert.IsNotNull(doc, "The result should not be null");
            Assert.AreEqual(company, doc.Company, "The company should be TALKOLA");
        }

        [TestMethod]
        public async Task GreaterInteger()
        {
            var docs = await service.AsQueryable<SolrDocument>().Where(x => x.Balance >= 1000 && x.Balance < 2000).ToListAsync();
            Assert.IsNotNull(docs, "The result should not be null");
            Assert.IsTrue(docs.Count > 0, "There should be at least one document");
            Assert.IsTrue(docs.All(x => x.Balance >= 1000 && x.Balance < 2000), "All documents should have Balance >= 1000 and balance < 2000");
        }

        [TestMethod]
        public async Task ContainsInMultilist()
        {
            var token = "Denmark";
            var docs = await service.AsQueryable<SolrDocument>().Where(x => x.Tags.Contains(token)).ToListAsync();
            Assert.IsNotNull(docs, "The result should not be null");
            Assert.IsTrue(docs.Count > 0, "There should be at least one document");
            Assert.IsTrue(docs.All(x => x.Tags.Contains(token)), $"All documents should have {token} in the tags");
        }

        [TestMethod]
        public async Task ContainsInString()
        {
            var token = "Maple Avenue";
            var docs = await service.AsQueryable<SolrDocument>().Where(x => x.Address.Contains(token)).ToListAsync();
            Assert.IsNotNull(docs, "The result should not be null");
            Assert.IsTrue(docs.Count > 0, "There should be at least one document");
            Assert.IsTrue(docs.All(x => x.Address.Contains(token)), $"All documents should have {token} in the address");
        }

        [TestMethod]
        public async Task ContainsInArray()
        {
            var array = new[] { 1, 5, 10 };
            var docs = await service.AsQueryable<SolrDocument>().Where(x => array.Contains(x.Index)).ToListAsync();
            Assert.IsNotNull(docs, "The result should not be null");
            Assert.IsTrue(docs.Count > 0, "There should be at least one document");
            Assert.IsTrue(docs.All(x => array.Contains(x.Index)), "All documents should have Index in the array");
        }

        [TestMethod]
        public async Task TakeTest()
        {
            var docs = await service.AsQueryable<SolrDocument>().Take(5).ToListAsync();
            Assert.IsNotNull(docs, "The result should not be null");
            Assert.AreEqual(5, docs.Count, "There should be 5 documents");
        }
        [TestMethod]
        public async Task SkipTest()
        {
            var docs = await service.AsQueryable<SolrDocument>().Take(5).Skip(10).ToListAsync();
            docs = docs.OrderBy(x => x.Index).ToList();
            Assert.IsNotNull(docs, "The result should not be null");
            Assert.AreEqual(5, docs.Count, "There should be 5 documents");  
          //  Assert.IsTrue(docs.All(x => x.Index > 10), "All documents should have Index > 10");
        }
    }
}