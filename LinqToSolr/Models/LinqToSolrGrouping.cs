using System;
using System.Linq.Expressions;

using LinqToSolr.Interfaces;
namespace LinqToSolr.Models
{
    public class LinqToSolrGrouping : ILinqToSolrGrouping
    {
        public Type Type { get; }
        public Expression Expression { get; }
        public string Field { get; set; }

        public LinqToSolrGrouping(string field, Expression expression)
        {
            Field = field;
            Expression = expression;
            Type = ((LambdaExpression)Expression).Body.Type;
        }
    }
}