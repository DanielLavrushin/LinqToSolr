using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using LinqToSolr.Helpers.Json;
using LinqToSolr.Interfaces;

namespace LinqToSolr.Models
{
    internal class ResponseError
    {
        [SolrField("msg")]
        internal string Message { get; set; }

        [SolrField("code")]
        internal int Code { get; set; }
    }
}
