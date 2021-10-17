using System;

namespace LinqToSolr.Interfaces
{
    public interface ILinqToSolrJoiner
    {
        string Field { get; set; }
        string ForeignKey { get; set; }
        string FieldKey { get; set; }
        Type PropertyRealType { get; set; }
    }
}
