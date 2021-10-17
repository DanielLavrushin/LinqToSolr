using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LinqToSolr
{
    [AttributeUsage(AttributeTargets.Property)]
    public class SolrFieldAttribute : Attribute
    {
        public string PropertyName;
        public string SearchFormat;
        public SolrFieldAttribute() { }

        public SolrFieldAttribute(string propertyName)
        {
            PropertyName = propertyName;
        }
        public SolrFieldAttribute(string propertyName, string searchFormat)
        {
            PropertyName = propertyName;
            SearchFormat = searchFormat;
        }
    }

    [AttributeUsage(AttributeTargets.Property)]
    public class SolrFieldIgnoreAttribute : Attribute
    {

    }
}
