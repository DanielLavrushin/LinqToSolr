using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using Newtonsoft.Json;
#if NET35
using LinqToSolr.Expressions;
#endif

namespace LinqToSolr.Data
{
    public class LinqSolrSelect
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
#if PORTABLE || NETCORE
            foreach (var p in baseType.GetRuntimeProperties())
#else
            foreach (var p in baseType.GetProperties())
#endif
            {
#if NET40 || NET35 || PORTABLE40
                var attr =
                    Attribute.GetCustomAttribute(p, typeof(JsonPropertyAttribute), true) as
                        JsonPropertyAttribute;
#else

                var attr = p.GetCustomAttribute<JsonPropertyAttribute>();
#endif
                if (attr != null)
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

#if NET35
        internal class GetAllMembersVisitor: ExpressionVisitorNet35
#else
        internal class GetAllMembersVisitor: System.Linq.Expressions.ExpressionVisitor
#endif
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
                return  node;
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
                return  node;
#else
                return base.VisitBinary(node);
#endif
            }


            internal static string GetName(MemberInfo member)
            {
                var prop = member;

#if NET40 || NET35 || PORTABLE40
                var dataMemberAttribute =
                    Attribute.GetCustomAttribute(prop, typeof(JsonPropertyAttribute), true) as
                        JsonPropertyAttribute;

#else

                var dataMemberAttribute = prop.GetCustomAttribute<JsonPropertyAttribute>();
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

    public class LinqToSolrQuery
    {
        public string Index { get; set; }
        internal string FilterUrl { get; set; }
        public ICollection<LinqToSolrFilter> Filters { get; set; }
        public ICollection<LinqToSolrFacet> Facets { get; set; }
        public ICollection<LinqToSolrFacet> FacetsToIgnore { get; set; }
        public ICollection<LinqToSolrSort> Sortings { get; set; }

        public bool IsGroupEnabled { get; set; }

        public ICollection<string> GroupFields { get; set; }
        public Type CurrentType { get; set; }
        public LinqSolrSelect Select { get; set; }

        internal ICollection<LinqToSolrJoiner> JoinFields { get; set; }

        public LinqToSolrQuery()
        {
            Filters = new List<LinqToSolrFilter>();
            Facets = new List<LinqToSolrFacet>();
            FacetsToIgnore = new List<LinqToSolrFacet>();
            Sortings = new List<LinqToSolrSort>();
            GroupFields = new List<string>();
            JoinFields = new List<LinqToSolrJoiner>();
        }

        public LinqToSolrQuery AddFilter(LambdaExpression field, params object[] values)
        {
            Filters.Add(LinqToSolrFilter.Create(field, values));

            return this;
        }
        public LinqToSolrQuery AddFilter(Type objectType, string field, params object[] values)
        {
            Filters.Add(LinqToSolrFilter.Create(objectType, field, values));

            return this;
        }
        public LinqToSolrQuery AddFilter<T>(string field, params object[] values)
        {
            Filters.Add(LinqToSolrFilter.Create<T>(field, values));

            return this;
        }


        public LinqToSolrQuery AddSorting(Expression field, SolrSortTypes order)
        {
            Sortings.Add(LinqToSolrSort.Create(field, order));

            return this;
        }

        public LinqToSolrQuery AddFacet(LambdaExpression field)
        {
            Facets.Add(LinqToSolrFacet.Create(field));

            return this;
        }

        public LinqToSolrFacet GetFacet(string propertyName)
        {
            return Facets.FirstOrDefault(x => x.Name == propertyName);
        }




    }
}