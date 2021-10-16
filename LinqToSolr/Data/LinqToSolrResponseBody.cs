using System;
using System.Collections.Generic;

using LinqToSolr.Helpers.Json;

namespace LinqToSolr.Data
{
    public class LinqToSolrResponseBody<T>
    {
        [JsonProperty("numFound")]
        public int Count { get; set; }

        [JsonProperty("start")]
        public int Start { get; set; }

        [JsonProperty("docs")]
        public ICollection<T> Documents { get; set; }

        public object GetList(Type elementType)
        {
            throw new NotImplementedException();
        }
    }
}
