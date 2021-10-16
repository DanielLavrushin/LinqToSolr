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
        public int id { get; set; }
        public string name { get; set; }
    }


    [TestFixture]
    public class LinqToSolrTests
    {
        private ILinqToSolrService _solr;

        [OneTimeSetUp]
        public void Init()
        {
            var config = new LinqToSolrRequestConfiguration("https://localhost:9193/")
                .MapIndexFor<TestCoreDoc>("testcore");
            _solr = new LinqToSolrService(config);
        }

        [Test]
        public void FirstOrDefaultTest()
        {
            var doc =_solr.AsQueryable<TestCoreDoc>().FirstOrDefault(x => x.id == 1);

            Assert.IsNotNull(doc);

        }



        [Test]
        public void AddOrUpdateTest()
        {
           var docs = new List<TestCoreDoc>();
            for (int i = 0; i <= 500; i++)
            {
                var name = RandomString(random.Next(5, 20));
                docs.Add(new TestCoreDoc{id =  i });
            }

            _solr.AddOrUpdate(docs.ToArray());

        }

        [Test]
        public void DeleteByIdTest()
        {
            var docs = new List<TestCoreDoc>();
            for (int i = 0; i <= 500; i++)
            {
                var name = RandomString(random.Next(5, 20));
                docs.Add(new TestCoreDoc { id = i });
            }

            _solr.Delete<TestCoreDoc>(docs.Select(x=>x.id).ToArray());

        }


        [Test]
        public void DeleteByQueryTest()
        {
        
            _solr.Delete<TestCoreDoc>(x=> x.id > 2);

        }

        private static Random random = new Random();
        private  string RandomString(int length)
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            return new string(Enumerable.Repeat(chars, length)
                .Select(s => s[random.Next(s.Length)]).ToArray());
        }

        [OneTimeTearDown]
        public void Finally()
        {

            System.Diagnostics.Debug.WriteLine(_solr.LastResponse.LastServiceUri);
        }
    }
}
