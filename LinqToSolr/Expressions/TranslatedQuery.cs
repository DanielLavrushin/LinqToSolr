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
    public interface ITranslatedQuery
    {
        int Skip { get; set; }
        int Take { get; set; }

        LambdaExpression SelectExpression { get; }
        IDictionary<string, SortingDirection> Sorting { get; }
        IDictionary<string, MemberInfo> Select { get; }
        IDictionary<string, object> Facets { get; }
        ICollection<string> Filters { get; }
        ICollection<string> Groups { get; }
        void AddFacet<TObject, TProperty>(Expression<Func<TObject, TProperty>> expression);
        void AddSorting(Expression expression, SortingDirection sortingDirection);
        void AddSelectField(Expression expression);
        void AddGrouping(Expression expression);
    }
    internal class TranslatedQuery : ITranslatedQuery
    {
        public int Skip { get; set; } = 0;
        public int Take { get; set; } = 100;
        public LambdaExpression SelectExpression { get; private set; }
        public IDictionary<string, SortingDirection> Sorting { get; } = new Dictionary<string, SortingDirection>();
        public IDictionary<string, MemberInfo> Select { get; } = new Dictionary<string, MemberInfo>();
        public ICollection<string> Filters { get; } = new List<string>();
        public ICollection<string> Groups { get; } = new List<string>();
        public IDictionary<string, object> Facets { get; } = new Dictionary<string, object>();

        public void AddFacet<TObject, TProperty>(Expression<Func<TObject, TProperty>> expression)
        {

            var visitor = new CollectMembersVisitor();
            visitor.Visit(expression);
            foreach (var member in visitor.Members)
            {
                Facets.Add(LinqToSolrFieldAttribute.GetFieldName(member), expression);
            }
        }

        public void AddSorting(Expression expression, SortingDirection sortingDirection)
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

        public void AddSelectField(Expression expression)
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

        public void AddGrouping(Expression expression)
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
