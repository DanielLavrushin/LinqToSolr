using LinqToSolr.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LinqToSolr.Tests.Models
{
    public class SolrDocument
    {
        [LinqToSolrField("id")]
        public Guid Id { get; set; }

        [LinqToSolrField("index")]
        public int Index { get; set; }

        [LinqToSolrField("isActive")]
        public bool IsActive { get; set; }

        [LinqToSolrField("isEnabled")]
        public bool? IsEnabled { get; set; }

#nullable enable
        [LinqToSolrField("name")]
        public string? Name { get; set; }
#nullable disable

        [LinqToSolrField("gender")]
        public string Gender { get; set; }
        [LinqToSolrField("company")]
        public string Company { get; set; }
        [LinqToSolrField("email")]
        public string Email { get; set; }
        [LinqToSolrField("picture")]
        public string Picture { get; set; }
        [LinqToSolrField("age")]
        public int Age { get; set; }
        [LinqToSolrField("balance")]
        public decimal Balance { get; set; }
        [LinqToSolrField("address")]
        public string Address { get; set; }
        [LinqToSolrField("latitude")]
        public double Latitude { get; set; }
        [LinqToSolrField("longitude")]
        public double Longitude { get; set; }
        [LinqToSolrField("tags")]
        public string[] Tags { get; set; }

        [LinqToSolrField("registered")]
        public DateTime Registered { get; set; }
    }
}
