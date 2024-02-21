using LinqToSolr.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LinqToSolr.Tests.Models
{
    internal class SolrDocument
    {
        [LinqToSolrField("id")]
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string Text { get; set; }
        public int Number { get; set; }
        public bool IsActive { get; set; }
        [LinqToSolrField("birthdaydate")]
        public DateTime Date { get; set; }
    }
}
