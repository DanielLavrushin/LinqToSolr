using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using LinqToSolr.Extensions;

namespace LinqToSolr.Attributes
{
    [AttributeUsage(AttributeTargets.Property)]
    public class LinqToSolrFieldAttribute : Attribute
    {
        public string PropertyName;
        public string SearchFormat;
        public LinqToSolrFieldAttribute(string propertyName) : this(propertyName, null)
        {
        }
        public LinqToSolrFieldAttribute(string propertyName, string searchFormat)
        {
            PropertyName = propertyName;
            SearchFormat = searchFormat;
        }

        public static string GetFieldName(MemberInfo member)
        {
            var name = member.Name;
            var attr = member.GetCustomAttribute<LinqToSolrFieldAttribute>();
            return attr == null ? name : attr.PropertyName;
        }
    }
}
