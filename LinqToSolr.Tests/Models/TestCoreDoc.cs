using System;


namespace LinqToSolr.Tests.Models
{
    public class TestCoreDoc
    {
        [SolrField("id")]
        public Guid Id { get; set; }

        [SolrField("name")]
        public string Name { get; set; }

        [SolrField("city")]
        public string City { get; set; }

        [SolrField("text")]
        public string Text { get; set; }

        [SolrField("lock")]
        public bool Locked { get; set; }

        [SolrField("parentid")]
        public Guid ParentId { get; set; }

        [SolrField("cities")]
        public string[] Sites { get; set; }

        [SolrField("path")]
        public Guid[] Pathes { get; set; }

        [SolrField("time")]
        public DateTime Time { get; set; }

        internal static TestCoreDoc New()
        {
            var doc = new TestCoreDoc()
            {
                Id = Guid.NewGuid(),
                Time = DateTime.Now,
                Name = "Document Name"
            };
            return doc;
        }
    }
}
