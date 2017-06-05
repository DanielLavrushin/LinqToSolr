using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using LinqToSolr.Data;
using LinqToSolr.Services;
using Newtonsoft.Json;
using System.ComponentModel;
using System.Globalization;
using System.Security;

using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters;
using Newtonsoft.Json.Utilities;
using Newtonsoft.Json.Serialization;

namespace LinqToSolr.Expressions
{

#if NET35
    internal class LinqToSolrQueryTranslator: LinqToSolr.Expressions.ExpressionVisitorNet35
#else
    internal class LinqToSolrQueryTranslator: System.Linq.Expressions.ExpressionVisitor
#endif
    {
        private StringBuilder sb;
        private bool _inRangeQuery;
        private bool _inRangeEqualQuery;
        private ILinqToSolrService _service;
        private bool _isRedudant;
        private ICollection<string> _sortings;
        private Type _elementType;

        internal LinqToSolrQueryTranslator(ILinqToSolrService query)
        {
            _service = query;
            _elementType = GetElementType(_service.ElementType);
            _sortings = new List<string>();
        }

        internal LinqToSolrQueryTranslator(ILinqToSolrService query, Type elementType)
        {
            _service = query;
            _elementType = GetElementType(elementType);
            _sortings = new List<string>();
            _elementType = elementType;
        }

        internal string GetFieldName(MemberInfo member)
        {


#if NET40 || NET35 || PORTABLE40
            var dataMemberAttribute =
                Attribute.GetCustomAttribute(member, typeof(JsonPropertyAttribute), true) as
                    JsonPropertyAttribute;

#else

            var dataMemberAttribute = member.GetCustomAttribute<JsonPropertyAttribute>();
#endif

            var fieldName = !string.IsNullOrEmpty(dataMemberAttribute?.PropertyName)
                ? dataMemberAttribute.PropertyName
                : member.Name;
            return fieldName;

        }



        private Type GetElementType(Type type)
        {

            if (type.Name == "IGrouping`2")
            {
#if PORTABLE || NETCORE
                return type.GetTypeInfo().IsGenericTypeDefinition 
    ? type.GetTypeInfo().GenericTypeParameters[1] 
    : type.GetTypeInfo().GenericTypeArguments[1];
#else
                return type.GetGenericArguments()[1];
#endif
            }
            return type;
        }

        internal string Translate(Expression expression)
        {
            _isRedudant = true;
            sb = new StringBuilder();
            Visit(expression);
            return sb.ToString();
        }


        private static Expression StripQuotes(Expression e)
        {
            while (e.NodeType == ExpressionType.Quote)
            {
                e = ((UnaryExpression)e).Operand;
            }
            return e;
        }


        protected override Expression VisitMethodCall(MethodCallExpression m)
        {
            if (m.Method.Name == "Where" || m.Method.Name == "First" || m.Method.Name == "FirstOrDefault")
            {
                var lambda = (LambdaExpression)StripQuotes(m.Arguments[1]);

                var solrQueryTranslator = new LinqToSolrQueryTranslator(_service);
                var fq = solrQueryTranslator.Translate(lambda.Body);
                sb.AppendFormat("&fq={0}", fq);

                var arr = StripQuotes(m.Arguments[0]);
                Visit(arr);
                return m;
            }
            if (m.Method.Name == "Take")
            {
                var takeNumber = (int)((ConstantExpression)m.Arguments[1]).Value;
                _service.Configuration.Take = takeNumber;
                Visit(m.Arguments[0]);
                return m;
            }
            if (m.Method.Name == "Skip")
            {
                var skipNumber = (int)((ConstantExpression)m.Arguments[1]).Value;
                _service.Configuration.Start = skipNumber;
                Visit(m.Arguments[0]);
                return m;
            }


            if (m.Method.Name == "OrderBy" || m.Method.Name == "ThenBy")
            {
                var lambda = (LambdaExpression)StripQuotes(m.Arguments[1]);

                _service.CurrentQuery.AddSorting(lambda.Body, SolrSortTypes.Asc);

                Visit(m.Arguments[0]);

                return m;
            }

            if (m.Method.Name == "OrderByDescending" || m.Method.Name == "ThenByDescending")
            {
                var lambda = (LambdaExpression)StripQuotes(m.Arguments[1]);
                _service.CurrentQuery.AddSorting(lambda.Body, SolrSortTypes.Desc);

                Visit(m.Arguments[0]);

                return m;
            }



            if (m.Method.Name == "Select")
            {
                _service.CurrentQuery.Select = new SolrSelect(StripQuotes(m.Arguments[1]));
                Visit(m.Arguments[0]);

                return m;
            }


            if (m.Method.Name == "Contains")
            {
                if (m.Method.DeclaringType == typeof(string))
                {
                    var str = string.Format("*{0}*", ((ConstantExpression)StripQuotes(m.Arguments[0])).Value);


                    Visit(BinaryExpression.Equal(m.Object, ConstantExpression.Constant(str)));

                    return m;
                }
                else
                {
                    var arr = (ConstantExpression)StripQuotes(m.Arguments[0]);
                    Expression lambda;

                    if (m.Arguments.Count == 2)
                    {
                        lambda = StripQuotes(m.Arguments[1]);
                        Visit(lambda);
                        Visit(arr);

                    }
                    else
                    {
                        var newExpr = Expression.Equal(m.Object, m.Arguments[0]);
                        Visit(newExpr);
                    }


                    return m;
                }
            }

            if (m.Method.Name == "StartsWith")
            {
                if (m.Method.DeclaringType == typeof(string))
                {
                    var str = string.Format("{0}*", ((ConstantExpression)StripQuotes(m.Arguments[0])).Value);
                    Visit(BinaryExpression.Equal(m.Object, ConstantExpression.Constant(str)));

                    return m;
                }
            }
            if (m.Method.Name == "EndsWith")
            {
                if (m.Method.DeclaringType == typeof(string))
                {
                    var str = string.Format("*{0}", ((ConstantExpression)StripQuotes(m.Arguments[0])).Value);
                    Visit(BinaryExpression.Equal(m.Object, ConstantExpression.Constant(str)));

                    return m;

                }
            }

            if (m.Method.Name == "GroupBy")
            {

                _service.CurrentQuery.IsGroupEnabled = true;
                var arr = StripQuotes(m.Arguments[1]);
#if PORTABLE || NETCORE
                var solrQueryTranslator =
new LinqToSolrQueryTranslator(_service, ((MemberExpression)((LambdaExpression)arr).Body).Member.DeclaringType);
#else
                var solrQueryTranslator = new LinqToSolrQueryTranslator(_service,
                    ((MemberExpression)((LambdaExpression)arr).Body).Member.ReflectedType);
#endif

                _service.CurrentQuery.GroupFields.Add(solrQueryTranslator.Translate(arr));
                Visit(m.Arguments[0]);

                return m;

                //throw new Exception("The method 'GroupBy' is not supported in Solr. For native FACETS support use SolrQuaryableExtensions.GroupBySolr instead.");
            }

            throw new NotSupportedException(string.Format("The method '{0}' is not supported", m.Method.Name));
        }

        protected override Expression VisitUnary(UnaryExpression u)
        {
            switch (u.NodeType)
            {
                case ExpressionType.Not:
                    sb.Append("-");
                    Visit(u.Operand);
                    break;

                case ExpressionType.Convert:
                    Visit(u.Operand);
                    break;
                default:
                    throw new NotSupportedException(
                        string.Format("The unary operator '{0}' is not supported", u.NodeType));
            }

            return u;
        }


        protected override Expression VisitBinary(BinaryExpression b)
        {
            _isRedudant = false;
            sb.Append("(");

            if (b.Left is ConstantExpression)
            {
#if NETCORE || PORTABLE40 || PORTABLE
                throw new Exception("Failed to parse expression. Ensure the Solr fields are always come in the left part of comparison.");
#else
                throw new System.Data.InvalidExpressionException(
                    "Failed to parse expression. Ensure the Solr fields are always come in the left part of comparison.");
#endif
            }
            Visit(b.Left);

            switch (b.NodeType)
            {
                case ExpressionType.And:
                case ExpressionType.AndAlso:
                    sb.Append(" AND ");
                    break;

                case ExpressionType.Or:
                case ExpressionType.OrElse:
                    sb.Append(" OR ");
                    break;

                case ExpressionType.Equal:
                    sb.Append(":");
                    break;
                case ExpressionType.NotEqual:
                    sb.Append(":=");
                    break;
                case ExpressionType.GreaterThanOrEqual:
                    sb.Append(":[");
                    _inRangeQuery = true;
                    break;
                case ExpressionType.LessThanOrEqual:
                    sb.Append(":[*");
                    _inRangeQuery = true;
                    break;
                case ExpressionType.GreaterThan:
                    sb.Append(":{");
                    _inRangeEqualQuery = true;
                    break;
                case ExpressionType.LessThan:
                    sb.Append(":{*");
                    _inRangeEqualQuery = true;
                    break;

                default:

                    throw new NotSupportedException(string.Format("The binary operator '{0}' is not supported",
                        b.NodeType));
            }

            Visit(b.Right);


            if (b.NodeType != ExpressionType.Equal &&
                b.NodeType != ExpressionType.NotEqual &&
                b.Right is MemberExpression &&
                (b.Left is BinaryExpression || b.Left.NodeType == ExpressionType.Call))
            {
                if (((MemberExpression)b.Right).Type == typeof(bool))
                {
                    sb.Append(":");
                    sb.Append(" True");
                }
            }

            sb.Append(")");

            return b;
        }


        protected override Expression VisitConstant(ConstantExpression c)
        {
            var q = c.Value as IQueryable;

            if (q != null)
            {
                // sb.Append(_elementType.Name);
            }
            else
            {
                //handle in range query
                if (_inRangeQuery || _inRangeEqualQuery)
                {
                    if (sb[sb.Length - 1] == '*')
                    {
                        sb.Append(" TO  ");
                        AppendConstValue(c.Value);
                    }
                    else
                    {
                        AppendConstValue(c.Value);
                        sb.Append(" TO *");
                    }
                    sb.Append(_inRangeEqualQuery ? "}" : "]");
                    _inRangeQuery = false;
                }
                else
                {
                    AppendConstValue(c.Value);
                }
            }

            return c;
        }

        private void AppendConstValue(object val)
        {


#if PORTABLE40
            var isArray = val.GetType().IsAssignableFrom(typeof(IEnumerable));
#elif PORTABLE || NETCORE
                var isArray = val.GetType().GetTypeInfo().IsArray;
#else
            var isArray = val.GetType().GetInterface("IEnumerable`1") != null;
#endif

            //Set date format of Solr 1995-12-31T23:59:59.999Z
            if (val.GetType() == typeof(DateTime))
            {
                sb.Append(((DateTime)val).ToString("yyyy-MM-ddThh:mm:ss.fffZ"));
            }
            else if (!(val is string) && isArray)
            {
                var array = (IEnumerable)val;
                var arrstring = string.Join(" OR ",
                    array.Cast<object>().Select(x => string.Format("\"{0}\"", x)).ToArray());
                sb.AppendFormat(": ({0})", arrstring);

            }
            else
            {
                if (val.ToString().Contains(" ") && !val.ToString().Contains("*"))
                {
                    sb.Append(string.Format("\"{0}\"", val));

                }
                else
                {
                    sb.Append(val);

                }
            }
        }

        protected override MemberAssignment VisitMemberAssignment(MemberAssignment node)
        {
            return base.VisitMemberAssignment(node);
        }

#if NET35
        protected override Expression VisitMemberAccess(MemberExpression m)
#else
        protected override Expression VisitMember(MemberExpression m)
#endif
        {
            if (m.Expression != null && m.Expression.NodeType == ExpressionType.Parameter)
            {

                var fieldName = GetFieldName(m.Member);
                sb.Append(fieldName);
                return m;
            }
            if (m.Expression != null)
            {
                if (m.Expression.NodeType == ExpressionType.Constant)
                {
                    var ce = (ConstantExpression)m.Expression;
                    sb.Append(ce.Value);
                }
                else if (m.Expression.NodeType == ExpressionType.MemberAccess)
                {

                    var ce = (MemberExpression)m.Expression;
#if NET40 || NET35 || PORTABLE40
                    var joinAttr =
                        Attribute.GetCustomAttribute(ce.Member, typeof(LinqToSolrForeignKeyAttribute), true) as
                            LinqToSolrForeignKeyAttribute;
#else
                    var joinAttr = ce.Member.GetCustomAttribute<LinqToSolrForeignKeyAttribute>();
#endif

                    if (joinAttr != null)
                    {
                        var fieldName = GetFieldName(m.Member);
                        var topType = ce.Expression as ParameterExpression;
                        if (topType != null)
                        {
                            var joiner = new LinqToSolrJoiner(ce.Member.Name, topType.Type);
                            var joinstr = string.Format("!join from={0} to={1} fromIndex={2}", joiner.FieldKey, joiner.ForeignKey, _service.Configuration.GetIndex(joiner.PropertyRealType));
                            sb.Append("{" + joinstr + "}" + fieldName);
                        }
                    }


                }

                return m;
            }

            throw new NotSupportedException(string.Format("The member '{0}' is not supported", m.Member.Name));
        }
    }
}
