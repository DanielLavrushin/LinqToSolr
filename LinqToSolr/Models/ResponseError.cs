using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using LinqToSolr.Helpers.Json;
using LinqToSolr.Interfaces;

namespace LinqToSolr.Models
{
    public class ResponseError
    {
        [SolrField("msg")]
        public string Message { get; set; }

        [SolrField("code")]
        public int Code { get; set; }
    }
}
