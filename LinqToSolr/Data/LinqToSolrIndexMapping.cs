using System;

namespace LinqToSolr.Data
{
    public class LinqToSolrIndexMapping
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