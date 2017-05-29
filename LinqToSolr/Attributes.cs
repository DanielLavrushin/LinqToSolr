using System;

namespace LinqToSolr
{
    public class LinqToSolrKeyAttribute : Attribute
    {
        public string Key;
        public LinqToSolrKeyAttribute()
        {
        }
        public LinqToSolrKeyAttribute(string key)
        {
            Key = key;
        }
    }

    public class LinqToSolrForeignKeyAttribute: Attribute
    {
        public string ForeignKey;
        public LinqToSolrForeignKeyAttribute(string foreignKey)
        {
            ForeignKey = foreignKey;
        }
    }
}
