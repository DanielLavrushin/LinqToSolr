using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using LinqToSolr.Helpers.Json;
using LinqToSolr.Interfaces;

namespace LinqToSolr.Models
{
    internal class ResponseHeader
    {
        [SolrField("responseHeader")]
        internal int Status { get; set; }

        [SolrField("qtime")]
        internal int Time { get; set; }

        [SolrField("params")]
        internal Dictionary<string, string> Query { get; set; }
    }
}
