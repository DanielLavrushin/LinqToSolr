using LinqToSolr.Attributes;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;

namespace LinqToSolr.Expressions
{
    public enum SortingDirection
    {
        ASC,
        DESC
    }
    internal class TranslatedQuery
    {
        public int Skip { get; set; } = 0;
        public int Take { get; set; } = 100;
        public LambdaExpression SelectExpression { get; private set; }
        internal IDictionary<string, SortingDirection> Sorting { get; } = new Dictionary<string, SortingDirection>();
        internal IDictionary<string, MemberInfo> Select { get; } = new Dictionary<string, MemberInfo>();
        internal ICollection<string> Filters { get; } = new List<string>();
        internal ICollection<string> Groups { get; } = new List<string>();
        internal void AddSorting(Expression expression, SortingDirection sortingDirection)
        {

            MemberExpression memberExpression = null;
            if (expression is UnaryExpression unaryExpression)
            {
                memberExpression = unaryExpression.Operand as MemberExpression;
            }
            else if (expression is LambdaExpression)
            {
                memberExpression = (expression as LambdaExpression).Body as MemberExpression;
            }
            else if (expression is MemberExpression)
            {
                memberExpression = expression as MemberExpression;
            }

            if (expression != null)
            {
                var fieldName = LinqToSolrFieldAttribute.GetFieldName(memberExpression.Member);
                Sorting.Add(fieldName, sortingDirection);
            }
        }

        internal void AddSelectField(Expression expression)
        {
            SelectExpression = expression as LambdaExpression;
            if (SelectExpression.Body.NodeType == ExpressionType.New)
            {
                var newExpression = (NewExpression)SelectExpression.Body;
                foreach (var argument in newExpression.Arguments)
                {
                    var member = (MemberExpression)argument;
                    Select.Add(LinqToSolrFieldAttribute.GetFieldName(member.Member), member.Member);
                }
            }
            else if (SelectExpression.Body.NodeType == ExpressionType.MemberAccess) // Single property
            {
                var member = (MemberExpression)SelectExpression.Body;
                Select.Add(LinqToSolrFieldAttribute.GetFieldName(member.Member), member.Member);
            }
        }

        internal void AddGrouping(Expression expression)
        {
            var lambda = expression as LambdaExpression;

            var visitor = new CollectMembersVisitor();
            visitor.Visit(lambda);
            foreach (var member in visitor.Members)
            {
                Groups.Add(LinqToSolrFieldAttribute.GetFieldName(member));
            }
        }

        class CollectMembersVisitor : ExpressionVisitor
        {
            public List<MemberInfo> Members { get; } = new List<MemberInfo>();
            protected override Expression VisitMember(MemberExpression node)
            {
                Members.Add(node.Member);
                return base.VisitMember(node);
            }
        }
    }
}
