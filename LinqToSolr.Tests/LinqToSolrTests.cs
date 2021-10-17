using System;
using System.Collections.Generic;
using System.Linq;

using LinqToSolr.Data;
using LinqToSolr.Services;

using NUnit.Framework;


namespace LinqToSolr.Tests
{
    public class TestCoreDoc
    {
        [SolrField("_group", "N")]
        public Guid Id { get; set; }

        [SolrField("_name")]
        public string Name { get; set; }

        [SolrField("_parent", "N")]
        public Guid ParentId { get; set; }

        [SolrField("site_sm")]
        public string[] Sites { get; set; }

        [SolrField("_path", "N")]
        public Guid[] Pathes { get; set; }

        [SolrField("_indextimestamp")]
        public DateTime Time { get; set; }
    }

    [TestFixture]
    public class LinqToSolrTests
    {
        private ILinqToSolrService _solr;

        [OneTimeSetUp]
        public void Init()
        {
            var config = new LinqToSolrConfiguration("https://localhost:9193/solr")
                .MapIndexFor<TestCoreDoc>("testcore");
            _solr = new LinqToSolrService(config);
        }

        [Test]
        public void FirstOrDefaultTest()
        {
            var id = new Guid("73966209f1b643ca853af1db1c9a654b");

            var doc = _solr.AsQueryable<TestCoreDoc>().Where(x => x.Id == id).First();

            Assert.IsNotNull(doc);
            Assert.IsTrue(doc.Id == id);

        }

        [Test]
        public void WhereQueryTest()
        {
            var ids = new[] { new Guid("67bc6a5df71a4b22a89f105b70f1c288"), new Guid("73966209f1b643ca853af1db1c9a654b") };

            var docs = _solr.AsQueryable<TestCoreDoc>().Where(x => ids.Contains(x.Id)).ToList();

            Assert.IsNotNull(docs);
            Assert.True(docs.Count == 2, "expected 2 documents");
            Assert.True(docs.All(x => ids.Contains(x.Id)));

        }


        //[Test]
        //public void AddOrUpdateTest()
        //{
        //    var docs = new List<TestCoreDoc>();
        //    for (int i = 0; i <= 500; i++)
        //    {
        //        var name = RandomString(random.Next(5, 20));
        //        docs.Add(new TestCoreDoc { id = i.ToString() });
        //    }

        //    //    _solr.AddOrUpdate(docs.ToArray());

        //}

        //[Test]
        //public void DeleteByIdTest()
        //{
        //    var docs = new List<TestCoreDoc>();
        //    for (int i = 0; i <= 500; i++)
        //    {
        //        var name = RandomString(random.Next(5, 20));
        //        docs.Add(new TestCoreDoc { id = i.ToString() });
        //    }

        //    //      _solr.Delete<TestCoreDoc>(docs.Select(x=>x.id).ToArray());

        //}


        //[Test]
        //public void DeleteByQueryTest()
        //{

        //    //         _solr.Delete<TestCoreDoc>(x=> x.id > 2);

        //}

        //private static Random random = new Random();
        //private string RandomString(int length)
        //{
        //    const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
        //    return new string(Enumerable.Repeat(chars, length)
        //        .Select(s => s[random.Next(s.Length)]).ToArray());
        //}

        [OneTimeTearDown]
        public void Finally()
        {

            //        System.Diagnostics.Debug.WriteLine(_solr.LastResponse.LastServiceUri);
        }
    }
}
