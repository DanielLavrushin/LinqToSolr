using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LinqToSolr.Interfaces
{
    public interface ILinqToSolrIndexMapping
    {
        Type Type { get; set; }
        string Index { get; set; }
    }
}
