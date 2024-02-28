using LinqToSolr.Extensions;
using LinqToSolr.Tests.Models;
using System.Diagnostics;

namespace LinqToSolr.Tests
{
    [TestClass]
    [DoNotParallelize]
    public class CrudTests : BaseTest
    {
        [ClassInitialize]
        public static void ClassInitialize(TestContext context)
        {
            DocId = Guid.NewGuid();
            Debug.WriteLine($"DocId: {DocId}");
        }

        [TestMethod("Create documents"), TestCategory("Crud"), Priority(10)]
        public async Task Crud1CreateDocsTest()
        {
            var doc = new SolrDocument
            {
                Index = 1000,
                Id = DocId,
                Name = "Test Document"
            };

            var result = await Query.AddOrUpdateAsync(doc);
            Assert.IsTrue(result, "The result should be true");
        }



        [TestMethod("Update documents"), TestCategory("Crud"), Priority(20)]
        public async Task Crud2UpdateSingleTest()
        {
            var doc = await Query.Where(d => d.Id == DocId).FirstOrDefaultAsync();
            Assert.IsNotNull(doc, "The result should not be null");
            Assert.AreEqual(DocId, doc.Id, $"The Id should be {DocId}");

            var oldName = doc.Name;
            var updatedName = oldName + " updated!";
            doc.Name = updatedName;
            var result = await Query.AddOrUpdateAsync(doc);
            Assert.IsTrue(result, "The result should be true");

            doc = await Query.Where(d => d.Id == DocId).FirstOrDefaultAsync();
            Assert.IsNotNull(doc, "The result should not be null");
            Assert.AreEqual(DocId, doc.Id, $"The Id should be {DocId}");
            Assert.IsTrue(doc.Name == updatedName, $"The name should  be the {updatedName}");

            doc.Name = oldName;
            result = await Query.AddOrUpdateAsync(doc);
            Assert.IsTrue(result, "The result should be true");

            doc = await Query.Where(d => d.Id == DocId).FirstOrDefaultAsync();
            Assert.IsNotNull(doc, "The result should not be null");
            Assert.AreEqual(DocId, doc.Id, $"The Id should be {DocId}");
            Assert.IsTrue(doc.Name == oldName, $"The name should  be the {oldName}");
        }

        [TestMethod("Delete documents"), TestCategory("Crud"), Priority(30)]
        public async Task Crud3DeleteDocTest()
        {
            var result = await Query.Where(d => d.Id == DocId).DeleteAsync();
            Assert.IsTrue(result, "The result should be true");

            var doc = await Query.Where(d => d.Id == DocId).FirstOrDefaultAsync();
            Assert.IsNull(doc, "The result should be null");
        }
    }
}
