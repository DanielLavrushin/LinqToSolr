using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LinqToSolr.Models
{
    public class ResponseGroupField<TResult>
    {
        [SolrField("matches")]
        public int Matches { get; set; }

        public ICollection<ResponseGroup<TResult>> Groups { get; set; }

    }

    public class ResponseGroup<TResult>
    {
        [SolrField("groupValue")]
        public object Id { get; set; }

        [SolrField("doclist")]
        public ResponseBody<TResult> Documents { get; set; }
    }
}
