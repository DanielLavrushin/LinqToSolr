using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

using LinqToSolr.Interfaces;

using LinqToSolr.Helpers;
namespace LinqToSolr.Data
{
    public class LinqSolrSelect : ILinqSolrSelect
    {
        public Type Type { get; set; }
        public Expression Expression { get; set; }

        static readonly Type[] PredefinedTypes = {
            typeof(Object),
            typeof(Boolean),
            typeof(Char),
            typeof(String),
            typeof(SByte),
            typeof(Byte),
            typeof(Int16),
            typeof(UInt16),
            typeof(Int32),
            typeof(UInt32),
            typeof(Int64),
            typeof(UInt64),
            typeof(Single),
            typeof(Double),
            typeof(Decimal),
            typeof(DateTime),
            typeof(TimeSpan),
            typeof(Guid),
            typeof(Guid?),
            typeof(Math),
            typeof(Convert)
        };


        public bool IsSingleField { get; set; }
        public LinqSolrSelect(Expression expression)
        {
            Expression = expression;
            Type = ((LambdaExpression)Expression).Body.Type;

        }
        public void CreateProxyType(Type baseType)
        {
            foreach (var p in baseType.GetRuntimeProperties())
            {
                var dataMemberAttribute = p.GetCustomAttribute<SolrFieldAttribute>();
                if (dataMemberAttribute != null)
                {
                    //    TypeDescriptor.AddAttributes(Type, attr);
                }
            }
        }

        public string GetSelectFields()
        {
            var str = "*";

            if (Expression != null)
            {
                str = GetAllMembersVisitor.GetMemberNames(((LambdaExpression)Expression).Body);
            }
            IsSingleField = !str.Contains(",") && PredefinedTypes.Contains(Type);
            return str;
        }


        internal class GetAllMembersVisitor : ExpressionVisitor
        {

            internal ICollection<string> Members;
            internal GetAllMembersVisitor()
            {
                Members = new List<string>();
            }

#if NET35
            protected override Expression VisitMemberAccess(MemberExpression node)
#else
            protected override Expression VisitMember(MemberExpression node)
#endif
            {
                Members.Add(GetName(node.Member));
#if NET35
                return node;
#else
                return base.VisitMember(node);
#endif
            }

#if NET35
            protected override Expression VisitBinary(BinaryExpression node)
#else
            protected override Expression VisitBinary(BinaryExpression node)
#endif
            {

                Visit(node.Left);
                Visit(node.Right);
#if NET35
                return node;
#else
                return base.VisitBinary(node);
#endif
            }


            internal static string GetName(MemberInfo member)
            {
                var prop = member;
#if NET45_OR_GREATER
                var dataMemberAttribute = prop.GetCustomAttribute<SolrFieldAttribute>();
#else
                var dataMemberAttribute = prop.GetCustomAttributes(typeof(SolrFieldAttribute), true).FirstOrDefault() as SolrFieldAttribute;

#endif
                return $"{prop.Name}:{(!string.IsNullOrEmpty(dataMemberAttribute?.PropertyName) ? dataMemberAttribute.PropertyName : prop.Name)}";
            }

            public static string GetMemberNames(Expression expression)
            {
                var gm = new GetAllMembersVisitor();
                gm.Visit(expression);

                return string.Join(",", gm.Members.ToArray());
            }

        }
    }
}