using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using LinqToSolr.Helpers.Json;
using LinqToSolr.Interfaces;

namespace LinqToSolr.Models
{
    internal class ResponseBody<T>
    {
        [SolrField("numFound")]
        internal int Count { get; set; }

        [SolrField("start")]
        internal int Start { get; set; }

        [SolrField("docs")]
        internal ICollection<T> Documents { get; set; }
    }
}
