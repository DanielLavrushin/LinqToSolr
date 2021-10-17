using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using LinqToSolr.Data;

namespace LinqToSolr.Interfaces
{
    public interface ILinqToSolrConfiguration
    {
        Uri EndPoint { get; set; }
        int Take { get; set; }
        int Start { get; set; }
        int FacetsLimit { get; set; }

        ICollection<ILinqToSolrIndexMapping> IndexMappings { get; set; }
        ILinqToSolrConfiguration MapIndexFor<T>(string index);
        ILinqToSolrConfiguration MapIndexFor(Type type, string index);

        string GetIndex<T>();
        string GetIndex(Type type);
    }
}
