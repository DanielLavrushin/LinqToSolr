using System;
using System.Collections.Generic;

using LinqToSolr.Helpers.Json;

namespace LinqToSolr.Data
{
    public class LinqToSolrResponseBody<T>
    {
        [SolrField("numFound")]
        public int Count { get; set; }

        [SolrField("start")]
        public int Start { get; set; }

        [SolrField("docs")]
        public ICollection<T> Documents { get; set; }

        public object GetList(Type elementType)
        {
            throw new NotImplementedException();
        }
    }
}
