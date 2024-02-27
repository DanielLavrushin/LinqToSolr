using LinqToSolr.Extensions;
using LinqToSolr.Tests.Models;

namespace LinqToSolr.Tests
{
    [TestClass]
    public class OrderTests : BaseTest
    {

        [TestMethod]
        public async Task OrderByAscTest()
        {
            var docs = await Query.OrderBy(x => x.Index).ToListAsync();
            Assert.IsNotNull(docs, "Docs should not be null");
            Assert.AreEqual(0, docs.First().Index, "First index should be 0");
        }
        [TestMethod]
        public async Task OrderThenByAscTest()
        {
            var docs = await Query.Take(999).OrderBy(x => x.Company).ThenBy(x => x.Name).ToListAsync();
            Assert.IsNotNull(docs, "Docs should not be null");
        }
        [TestMethod]
        public async Task OrderByDescTest()
        {
            var docs = await Query.OrderByDescending(x => x.Index).ToListAsync();
            Assert.IsNotNull(docs, "Docs should not be null");
            Assert.IsTrue(docs.First().Index > docs.Last().Index, "First index should be greater than last index");
        }

        [TestMethod]
        public async Task OrderWithWhereTest()
        {
            var balanceFrom = 1000;
            var balanceTo = 1500;
            var docs = await Query.Where(x => x.Company != null && x.Balance > balanceFrom && x.Balance <= balanceTo).OrderByDescending(x => x.Balance).ToListAsync();
            Assert.IsNotNull(docs, "Docs should not be null");
            Assert.IsTrue(docs.First().Balance > docs.Last().Balance, "First balance should be greater than last balance");
            Assert.IsTrue(docs.Last().Balance > balanceFrom, $"Last balance should be greater than {balanceFrom}");
            Assert.IsTrue(docs.First().Balance <= balanceTo, $"First balance should be less than or equal to {balanceTo}");
            Assert.IsTrue(docs.All(x => x.Company != null), "All docs should have company");
        }
    }
}
