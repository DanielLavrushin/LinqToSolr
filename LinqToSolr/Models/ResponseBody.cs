using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using LinqToSolr.Helpers.Json;
using LinqToSolr.Interfaces;

namespace LinqToSolr.Models
{
    public class ResponseBody<T>
    {
        [SolrField("numFound")]
        public int Count { get; set; }

        [SolrField("start")]
        public int Start { get; set; }

        [SolrField("docs")]
        public ICollection<T> Documents { get; set; }
    }
}
