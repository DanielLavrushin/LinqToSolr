using System;

namespace LinqToSolr
{


    public class LinqToSolrJsonPropertyAttribute : Attribute
    {

    }

    [AttributeUsage(AttributeTargets.Property)]
    public class LinqToSolrKeyAttribute : Attribute
    {
        public string Key;

        public LinqToSolrKeyAttribute(string key)
        {
            Key = key;
        }
    }

    [AttributeUsage(AttributeTargets.Property)]
    public class LinqToSolrForeignKeyAttribute : Attribute
    {
        public string ForeignKey;
        public LinqToSolrForeignKeyAttribute(string foreignKey)
        {
            ForeignKey = foreignKey;
        }
    }
}
