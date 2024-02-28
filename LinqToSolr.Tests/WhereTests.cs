using LinqToSolr.Extensions;
using LinqToSolr.Tests.Models;

namespace LinqToSolr.Tests
{
    [TestClass]
    public class WhereTests : BaseTest
    {

        [TestMethod]
        public async Task EqualIntTest()
        {
            var id = 1;
            var doc = await Service.AsQueryable<SolrDocument>().Where(x => x.Index == id).FirstOrDefaultAsync();
            Assert.IsNotNull(doc, "The result should not be null");
            Assert.AreEqual(id, doc.Index, "The index should be 1");
        }

        [TestMethod]
        public async Task EqualIntOrTest()
        {
            var docs = await Service.AsQueryable<SolrDocument>().Where(x => x.Index == 20 || x.Index == 10).ToListAsync();
            Assert.IsNotNull(docs, "The result should not be null");
            Assert.IsTrue(docs.Count > 0, "There should be at least one document");
            Assert.IsTrue(docs.Any(x => x.Index == 20 || x.Index == 10), "There should be at least one document with index 20 or 10");
        }
        [TestMethod]
        public async Task EqualStringTest()
        {
            var company = "DIGIQUE";
            var doc = await Service.AsQueryable<SolrDocument>().Where(x => x.Company == company).FirstOrDefaultAsync();
            Assert.IsNotNull(doc, "The result should not be null");
            Assert.AreEqual(company, doc.Company, "The company should be DIGIQUE");
        }
        [TestMethod]
        public async Task EqualBoolTest()
        {
            var docs1 = await Service.AsQueryable<SolrDocument>().Take(100).Where(x => x.IsActive).ToListAsync();
            var docs2 = await Service.AsQueryable<SolrDocument>().Take(100).Where(x => x.IsActive == true).ToListAsync();
            Assert.IsNotNull(docs1, "The result should not be null");
            Assert.IsNotNull(docs2, "The result should not be null");
            Assert.IsTrue(docs1.Count > 0, "There should be at least one document");
            Assert.IsTrue(docs2.Count > 0, "There should be at least one document");
            Assert.IsTrue(docs1.All(x => x.IsActive), "All documents should have IsActive == true");
            Assert.IsTrue(docs2.All(x => x.IsActive), "All documents should have IsActive == true");
        }

        [TestMethod]
        public async Task NullableBoolTest()
        {
            bool? someBool = null;
            //TODO: does not work, need to fix
            // var docs1 = await service.AsQueryable<SolrDocument>().Take(100).Where(x => (bool)x.IsEnabled).ToListAsync();
            var docs2 = await Service.AsQueryable<SolrDocument>().Take(100).Where(x => x.IsEnabled == true).ToListAsync();
            var docs3 = await Service.AsQueryable<SolrDocument>().Take(100).Where(x => x.IsEnabled == false).ToListAsync();
            var docs4 = await Service.AsQueryable<SolrDocument>().Take(100).Where(x => x.IsEnabled == someBool).ToListAsync();
            var docs5 = await Service.AsQueryable<SolrDocument>().Take(100).Where(x => x.IsEnabled == someBool.GetValueOrDefault()).ToListAsync();
            //Assert.IsNotNull(docs2, "The result should not be null");
            //Assert.IsNotNull(docs3, "The result should not be null");
            //Assert.IsNotNull(docs4, "The result should not be null");
            //Assert.IsTrue(docs2.Count > 0, "There should be at least one document");
            //Assert.IsTrue(docs3.Count > 0, "There should be at least one document");
            //Assert.IsTrue(docs4.Count > 0, "There should be at least one document");
            //Assert.IsTrue(docs2.All(x => x.IsEnabled == true), "All documents should have IsEnabled == true");
            //Assert.IsTrue(docs3.All(x => x.IsEnabled == false), "All documents should have IsEnabled == false");
            //Assert.IsTrue(docs4.All(x => x.IsEnabled == someBool), "All documents should have IsEnabled == true");
            //Assert.IsFalse(docs5.Any(), "There should be no documents");
        }
        [TestMethod]
        public async Task NotEqualNullTest()
        {
            var docs = await Service.AsQueryable<SolrDocument>().Where(x => x.Company != null).ToListAsync();
            Assert.IsNotNull(docs, "The result should not be null");
            Assert.IsTrue(docs.Count > 0, "There should be at least one document");
            Assert.IsTrue(docs.All(x => x.Company != null), "All documents should have company != null");
        }

        [TestMethod]
        public async Task EqualNullTest()
        {
            var docs = await Service.AsQueryable<SolrDocument>().Where(x => x.Company == null).ToListAsync();

            Assert.IsNotNull(docs, "The result should not be null");
            Assert.IsTrue(docs.Count > 0, "There should be at least one document");
            Assert.IsTrue(docs.All(x => x.Company == null), "All documents should have company == null");
        }

        [TestMethod]
        public async Task NotEqualStringTest()
        {
            var company = "DIGIQUE";
            var docs = await Service.AsQueryable<SolrDocument>().Take(100).Where(x => x.Company != company).ToListAsync();
            Assert.IsNotNull(docs, "The result should not be null");
            Assert.IsTrue(docs.Count > 0, "There should be at least one document");
            Assert.IsTrue(docs.All(x => x.Company != company), "All documents should have company != DIGIQUE");
        }
        [TestMethod]
        public async Task NotEqualBoolTest()
        {
            var docs1 = await Service.AsQueryable<SolrDocument>().Take(100).Where(x => !x.IsActive).ToListAsync();
            var docs2 = await Service.AsQueryable<SolrDocument>().Take(100).Where(x => x.IsActive != true).ToListAsync();
            var docs3 = await Service.AsQueryable<SolrDocument>().Take(100).Where(x => x.IsActive != false).ToListAsync();
            Assert.IsNotNull(docs1, "The result should not be null");
            Assert.IsNotNull(docs2, "The result should not be null");
            Assert.IsNotNull(docs3, "The result should not be null");
            Assert.IsTrue(docs1.Count > 0, "There should be at least one document");
            Assert.IsTrue(docs2.Count > 0, "There should be at least one document");
            Assert.IsTrue(docs3.Count > 0, "There should be at least one document");
            Assert.IsTrue(docs1.All(x => !x.IsActive), "All documents should have IsActive == false");
            Assert.IsTrue(docs2.All(x => !x.IsActive), "All documents should have IsActive == false");
            Assert.IsTrue(docs3.All(x => x.IsActive), "All documents should have IsActive == true");
        }
        [TestMethod]
        public async Task NotContainsStringTest()
        {
            var token = "Maple Avenue";
            var docs = await Service.AsQueryable<SolrDocument>().Where(x => x.Address != null && !x.Address.Contains(token)).ToListAsync();
            Assert.IsNotNull(docs, "The result should not be null");
            Assert.IsTrue(docs.Count > 0, "There should be at least one document");
            Assert.IsTrue(docs.All(x => !x.Address.Contains(token)), $"All documents should have Address not containing {token}");
        }

        [TestMethod]
        public async Task NotStartsWithStringTest()
        {
            var token = "Norton";
            var docs = await Service.AsQueryable<SolrDocument>().Where(x => x.Name != null && !x.Name.StartsWith(token)).ToListAsync();
            Assert.IsNotNull(docs, "The result should not be null");
            Assert.IsTrue(docs.Count > 0, "There should be at least one document");
            Assert.IsTrue(docs.All(x => !x.Name.StartsWith(token)), $"All documents should have Name not starting with {token}");
        }

        [TestMethod]
        public async Task GreaterLessOrEqualIntegerTest()
        {
            var docs = await Service.AsQueryable<SolrDocument>().Where(x => x.Balance >= 1000 && x.Balance <= 2000).ToListAsync();
            Assert.IsNotNull(docs, "The result should not be null");
            Assert.IsTrue(docs.Count > 0, "There should be at least one document");
            Assert.IsTrue(docs.All(x => x.Balance >= 1000 && x.Balance <= 2000), "All documents should have Balance >= 1000 and balance <= 2000");
        }
        [TestMethod]
        public async Task GreaterLessIntegerTest()
        {
            var docs = await Service.AsQueryable<SolrDocument>().Where(x => x.Balance > 1000 && x.Balance < 2000).ToListAsync();
            Assert.IsNotNull(docs, "The result should not be null");
            Assert.IsTrue(docs.Count > 0, "There should be at least one document");
            Assert.IsTrue(docs.All(x => x.Balance > 1000 && x.Balance < 2000), "All documents should have Balance > 1000 and balance < 2000");
            Assert.IsTrue(docs.All(x => x.Balance != 1000 && x.Balance != 2000), "All documents should have Balance != 1000 and balance != 2000");
        }

        [TestMethod]
        public async Task ContainsInMultilistTest()
        {
            var token = "Denmark";
            var docs = await Service.AsQueryable<SolrDocument>().Where(x => x.Tags.Contains(token)).ToListAsync();
            Assert.IsNotNull(docs, "The result should not be null");
            Assert.IsTrue(docs.Count > 0, "There should be at least one document");
            Assert.IsTrue(docs.All(x => x.Tags.Contains(token)), $"All documents should have {token} in the tags");
        }

        [TestMethod]
        public async Task ContainsInStringTest()
        {
            var token = "Maple Avenue";
            var docs = await Service.AsQueryable<SolrDocument>().Where(x => x.Address.Contains(token)).ToListAsync();
            Assert.IsNotNull(docs, "The result should not be null");
            Assert.IsTrue(docs.Count > 0, "There should be at least one document");
            Assert.IsTrue(docs.All(x => x.Address.Contains(token)), $"All documents should have {token} in the address");
        }
        [TestMethod]
        public async Task EndsWithStringTest()
        {
            var token = "Hodge";
            var docs = await Service.AsQueryable<SolrDocument>().Where(x => x.Name.EndsWith(token)).ToListAsync();
            Assert.IsNotNull(docs, "The result should not be null");
            Assert.IsTrue(docs.Count > 0, "There should be at least one document");
            Assert.IsTrue(docs.All(x => x.Name.EndsWith(token)), $"All documents should end with {token} in the name");
        }
        [TestMethod]
        public async Task StartsWithStringTest()
        {
            var token = "Selma";
            var docs = await Service.AsQueryable<SolrDocument>().Where(x => x.Name.StartsWith(token)).ToListAsync();
            Assert.IsNotNull(docs, "The result should not be null");
            Assert.IsTrue(docs.Count > 0, "There should be at least one document");
            Assert.IsTrue(docs.All(x => x.Name.StartsWith(token)), $"All documents should start with {token} in the name");
        }

        [TestMethod]
        public async Task ContainsNotInArrayTest()
        {
            var array = new[] { 1, 5, 10 };
            var docs = await Service.AsQueryable<SolrDocument>().Where(x => !array.Contains(x.Index)).ToListAsync();
            Assert.IsNotNull(docs, "The result should not be null");
            Assert.IsTrue(docs.Count > 0, "There should be at least one document");
            Assert.IsTrue(docs.All(x => !array.Contains(x.Index)), "All documents should not have Index in the array");
        }

        [TestMethod]
        public async Task ContainsInArrayTest()
        {
            var array = new[] { 1, 5, 10 };
            var docs = await Service.AsQueryable<SolrDocument>().Where(x => array.Contains(x.Index)).ToListAsync();
            Assert.IsNotNull(docs, "The result should not be null");
            Assert.IsTrue(docs.Count > 0, "There should be at least one document");
            Assert.IsTrue(docs.All(x => array.Contains(x.Index)), "All documents should have Index in the array");
        }
        [TestMethod]
        public async Task NotContainsInMultiFieldTest()
        {
            var token = "Hungary";
            var docs = await Service.AsQueryable<SolrDocument>().Where(x => x.Tags != null && !x.Tags.Contains(token)).ToListAsync();
            Assert.IsNotNull(docs, "The result should not be null");
            Assert.IsTrue(docs.Count > 0, "There should be at least one document");
            Assert.IsTrue(docs.All(x => !x.Tags.Contains(token)), $"All documents should not have {token} in the tags");
        }
        [TestMethod]
        public async Task TakeTest()
        {
            var docsNum = 50;
            var docs = await Service.AsQueryable<SolrDocument>().Take(docsNum).ToListAsync();
            Assert.IsNotNull(docs, "The result should not be null");
            Assert.AreEqual(docsNum, docs.Count, $"There should be {docsNum} documents");
        }
        [TestMethod]
        public async Task SkipTest()
        {
            var docs = await Service.AsQueryable<SolrDocument>().Skip(10).Take(5).ToListAsync();
            docs = docs.OrderBy(x => x.Index).ToList();
            Assert.IsNotNull(docs, "The result should not be null");
            Assert.AreEqual(5, docs.Count, "There should be 5 documents");
            //  Assert.IsTrue(docs.All(x => x.Index > 10), "All documents should have Index > 10");
        }

        [TestMethod]
        public async Task MultipleFiltersTest()
        {
            var query = Service.AsQueryable<SolrDocument>();
            query = query.Where(x => x.Index > 10);
            query = query.Where(x => x.Index < 50);
            query = query.Where(x => x.Name.StartsWith("A"));
            var docs = await query.ToListAsync();
            Assert.IsNotNull(docs, "The result should not be null");
            Assert.IsTrue(docs.Count > 0, "There should be at least one document");
            Assert.IsTrue(docs.All(x => x.Index > 10 && x.Index < 50 && x.Name.StartsWith("A")), "All documents should have Index > 10 and Index < 50 and Name starts with A");
        }
    }
}