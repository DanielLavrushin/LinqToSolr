using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using Newtonsoft.Json;

namespace LinqToSolr.Data
{
    public class SolrSelect
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
        public SolrSelect(Expression expression)
        {
            Expression = expression;
            Type = ((LambdaExpression)Expression).Body.Type;

        }
        public void CreateProxyType(Type baseType)
        {
            foreach (var p in baseType.GetProperties())
            {
                var attr = p.GetCustomAttribute<JsonPropertyAttribute>();
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

        internal class GetAllMembersVisitor: ExpressionVisitor
        {
            internal ICollection<string> Members;
            internal GetAllMembersVisitor()
            {
                Members = new List<string>();
            }


            protected override Expression VisitMember(MemberExpression node)
            {
                Members.Add(GetName(node.Member));
                return base.VisitMember(node);
            }

            protected override Expression VisitBinary(BinaryExpression node)
            {

                Visit(node.Left);
                Visit(node.Right);

                return base.VisitBinary(node);
            }


            internal static string GetName(MemberInfo member)
            {
                var prop = member;
                var dataMemberAttribute = prop.GetCustomAttribute<JsonPropertyAttribute>();

                return $"{prop.Name}:{(!string.IsNullOrEmpty(dataMemberAttribute?.PropertyName) ? dataMemberAttribute.PropertyName : prop.Name)}";
            }

            public static string GetMemberNames(Expression expression)
            {
                var gm = new GetAllMembersVisitor();
                gm.Visit(expression);

                return string.Join(",", gm.Members);
            }

        }
    }

    public class LinqToSolrQuery
    {
        public string Index { get; set; }
        internal string FilterUrl { get; set; }
        public ICollection<LinqToSolrFilter> Filters { get; set; }
        public ICollection<LinqToSolrFacet> Facets { get; set; }
        public ICollection<LinqToSolrSort> Sortings { get; set; }

        public bool IsGroupEnabled { get; set; }

        public ICollection<string> GroupFields { get; set; }
        public Type CurrentType { get; set; }
        public SolrSelect Select { get; set; }


        public LinqToSolrQuery()
        {
            Filters = new List<LinqToSolrFilter>();
            Facets = new List<LinqToSolrFacet>();
            Sortings = new List<LinqToSolrSort>();
            GroupFields = new List<string>();
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