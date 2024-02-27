using LinqToSolr.Extensions;
using LinqToSolr.Tests.Models;

namespace LinqToSolr.Tests
{
    [TestClass]
    [DoNotParallelize]
    public class CrudTests : BaseTest
    {
        [TestMethod]
        public async Task DoUpdateSingleTest()
        {
            var docIndex = 1;
            var prevName = "Norton Guthrie";
            var updateName = "Norton Guthrie Updated!";
            var doc = await Query.Where(d => d.Index == docIndex).FirstOrDefaultAsync();
            Assert.IsNotNull(doc, "The result should not be null");
            Assert.AreEqual(docIndex, doc.Index, "The index should be the same");
            Assert.IsTrue(doc.Name == prevName, $"The name should  be the {prevName}");

            doc.Name = updateName;
            var result = await Query.AddOrUpdateAsync(doc);
            Assert.IsTrue(result, "The result should be true");

            doc = await Query.Where(d => d.Index == docIndex).FirstOrDefaultAsync();
            Assert.IsNotNull(doc, "The result should not be null");
            Assert.AreEqual(docIndex, doc.Index, "The index should be the same");
            Assert.IsTrue(doc.Name == updateName, $"The name should  be the {updateName}");

            doc.Name = prevName;
            result = await Query.AddOrUpdateAsync(doc);
            Assert.IsTrue(result, "The result should be true");

            doc = await Query.Where(d => d.Index == docIndex).FirstOrDefaultAsync();
            Assert.IsNotNull(doc, "The result should not be null");
            Assert.AreEqual(docIndex, doc.Index, "The index should be the same");
            Assert.IsTrue(doc.Name == prevName, $"The name should  be the {prevName}");
        }
    }
}
