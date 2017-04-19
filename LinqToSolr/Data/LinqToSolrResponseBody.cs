using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace LinqToSolr.Data
{
    public class LinqToSolrResponseBody
    {
        [JsonProperty("numFound")]
        public int Count { get; set; }

        [JsonProperty("start")]
        public int Start { get; set; }

        [JsonProperty("docs")]
        public ICollection<object> Documents { get; set; }

        public object GetList(Type elementType)
        {
            throw new NotImplementedException();
        }
    }
}
