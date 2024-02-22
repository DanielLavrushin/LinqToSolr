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
        public Guid Guid { get; set; }
        public int Index { get; set; }
        public bool IsActive { get; set; }
        public string Name { get; set; }
        public string Gender { get; set; }
        public string Company { get; set; }
        public string Email { get; set; }
        public string Picture { get; set; }
        public int Age { get; set; }
        public int Balance { get; set; }
        public string Address { get; set; }
        public double Latitude { get; set; }
        public string Longitude { get; set; }
        public string[] Tags { get; set; }

        [LinqToSolrField("registered")]
        public DateTime Registered { get; set; }
    }
}
