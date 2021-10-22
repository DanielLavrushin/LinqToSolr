using System;


namespace LinqToSolr.Tests.Models
{
    public class TestCoreDoc
    {
        [SolrField("_group")]
        public Guid Id { get; set; }

        [SolrField("_name")]
        public string Name { get; set; }

        [SolrField("_parent")]
        public Guid ParentId { get; set; }

        [SolrField("site_sm")]
        public string[] Sites { get; set; }

        [SolrField("_path")]
        public Guid[] Pathes { get; set; }

        [SolrField("_indextimestamp")]
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
