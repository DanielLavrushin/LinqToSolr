using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

using LinqToSolr.Helpers;

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

        public static string GetFieldName(MemberInfo member)
        {
            var name = member.Name;
            var attr = member.GetCustomAttribute<SolrFieldAttribute>();
            return attr == null ? name : attr.PropertyName;
        }
    }

    [AttributeUsage(AttributeTargets.Property)]
    public class SolrFieldIgnoreAttribute : Attribute
    {

    }
}
