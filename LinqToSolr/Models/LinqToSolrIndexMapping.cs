using System;

using LinqToSolr.Interfaces;

namespace LinqToSolr.Models
{
    public class LinqToSolrIndexMapping : ILinqToSolrIndexMapping
    {
        public Type Type { get; set; }
        public string Index { get; set; }

        public LinqToSolrIndexMapping(Type type, string index)
        {
            Type = type;
            Index = index;

        }
    }
}