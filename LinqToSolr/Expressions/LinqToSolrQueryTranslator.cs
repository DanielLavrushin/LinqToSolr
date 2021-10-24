using System;
using System.Collections;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;

using LinqToSolr.Data;
using LinqToSolr.Interfaces;
using LinqToSolr.Services;
using LinqToSolr.Helpers;

namespace LinqToSolr.Expressions
{
    internal class LinqToSolrQueryTranslator : ExpressionVisitor
    {
        private StringBuilder sb;
        private bool _inRangeQuery;
        private bool _inRangeEqualQuery;
        private ILinqToSolrQuery query;
        private bool _isNotEqual;
        private Type _elementType;
        internal bool IsMultiList;


        internal LinqToSolrQueryTranslator(ILinqToSolrQuery query)
        {
            this.query = query;
            _elementType = GetElementType(TypeSystem.GetElementType(query.Expression.Type));
        }

        internal string GetFieldName(MemberInfo member, out string format)
        {
            format = null;
            var fieldName = member.GetSolrFieldName(out format);

            if (query.FacetsToIgnore.Any())
            {
                var facetToIgnoreName = query.FacetsToIgnore.FirstOrDefault(x => x.Field == fieldName);
                if (!string.IsNullOrEmpty(facetToIgnoreName?.Field))
                {
                    return string.Format("{{!tag={0}}}{1}", facetToIgnoreName.Field, fieldName);
                }
            }
            return fieldName;
        }



        private Type GetElementType(Type type)
        {

            if (type.Name == "IGrouping`2")
            {
#if NETSTANDARD
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
            if (m.Method.Name == nameof(Enumerable.Where) || m.Method.Name == nameof(Enumerable.First) || m.Method.Name == nameof(Enumerable.FirstOrDefault))
            {
                var lambda = (LambdaExpression)StripQuotes(m.Arguments[1]);

                var solrQueryTranslator = new LinqToSolrQueryTranslator(query);
                var fq = solrQueryTranslator.Translate(lambda.Body);
                sb.AppendFormat("&fq={0}", fq);
                query.Filters.Add(LinqToSolrFilter.Create(fq));
                var arr = StripQuotes(m.Arguments[0]);
                Visit(arr);
                return m;
            }

            if (m.Method.Name == nameof(Enumerable.Take))
            {
                var takeNumber = (int)((ConstantExpression)m.Arguments[1]).Value;
                query.Take = takeNumber;
                Visit(m.Arguments[0]);
                return m;
            }

            if (m.Method.Name == nameof(Enumerable.Skip))
            {
                var skipNumber = (int)((ConstantExpression)m.Arguments[1]).Value;
                query.Start = skipNumber;
                Visit(m.Arguments[0]);
                return m;
            }

            if (m.Method.Name == nameof(Enumerable.OrderBy) || m.Method.Name == nameof(Enumerable.ThenBy))
            {
                var lambda = (LambdaExpression)StripQuotes(m.Arguments[1]);

                query.Sortings.Add(LinqToSolrSort.Create(lambda.Body, SolrSortTypes.Asc));
                Visit(m.Arguments[0]);
                return m;
            }

            if (m.Method.Name == nameof(Enumerable.OrderByDescending) || m.Method.Name == nameof(Enumerable.ThenByDescending))
            {
                var lambda = (LambdaExpression)StripQuotes(m.Arguments[1]);
                query.Sortings.Add(LinqToSolrSort.Create(lambda.Body, SolrSortTypes.Desc));
                Visit(m.Arguments[0]);
                return m;
            }

            if (m.Method.Name == nameof(Enumerable.Select))
            {
                query.Select = new LinqSolrSelect(StripQuotes(m.Arguments[1]));
                Visit(m.Arguments[0]);
                return m;
            }

            if (m.Method.Name == nameof(string.Contains))
            {
                if (m.Method.DeclaringType == typeof(string))
                {
                    var str = string.Format("*{0}*", ((ConstantExpression)StripQuotes(m.Arguments[0])).Value);
                    Visit(BinaryExpression.Equal(m.Object, ConstantExpression.Constant(str)));
                    return m;
                }
                else
                {
                    var arr = StripQuotes(m.Arguments[0]);
                    Expression lambda;

                    if (m.Arguments[0].NodeType == ExpressionType.Constant)
                    {
                        lambda = StripQuotes(m.Arguments[1]);
                        Visit(lambda);
                        Visit(arr);

                    }
                    else
                    {
                        Visit(m.Arguments[0]);
                        sb.Append(":");
                        Visit(StripQuotes(m.Arguments[1]));
                    }


                    return m;
                }
            }

            if (m.Method.Name == nameof(string.StartsWith))
            {
                if (m.Method.DeclaringType == typeof(string))
                {
                    var str = string.Format("{0}*", ((ConstantExpression)StripQuotes(m.Arguments[0])).Value);
                    Visit(BinaryExpression.Equal(m.Object, ConstantExpression.Constant(str)));

                    return m;
                }
            }
            if (m.Method.Name == nameof(string.EndsWith))
            {
                if (m.Method.DeclaringType == typeof(string))
                {
                    var str = string.Format("*{0}", ((ConstantExpression)StripQuotes(m.Arguments[0])).Value);
                    Visit(BinaryExpression.Equal(m.Object, ConstantExpression.Constant(str)));

                    return m;

                }
            }

            if (m.Method.Name == nameof(Enumerable.GroupBy))
            {
                var arr = StripQuotes(m.Arguments[1]);
                var solrQueryTranslator = new LinqToSolrQueryTranslator(query);
                query.GroupFields.Add(solrQueryTranslator.Translate(arr));
                Visit(m.Arguments[0]);

                return m;

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
            sb.Append("(");

            if (b.Left is ConstantExpression)
            {
                throw new System.Data.InvalidExpressionException("Failed to parse expression. Ensure the Solr fields are always come in the left part of comparison.");
            }
            if (b.NodeType == ExpressionType.NotEqual)
            {
                _isNotEqual = true;
            }
            Visit(b.Left);

            if (_isNotEqual && b.Left.NodeType == ExpressionType.Call)
            {
                _isNotEqual = false;
                sb.Append(")");
                return b;
            }

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
                    sb.Append(":");
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
#if NETSTANDARD
            var isArray = val.GetType().GetTypeInfo().IsArray;
#else
            var isArray = val.GetType().GetInterface("IEnumerable`1") != null;
#endif
            var format = !string.IsNullOrEmpty(formatValue) ? "{0:" + formatValue + "}" : "{0}";
            //Set date format of Solr 1995-12-31T23:59:59.999Z
            if (val.GetType() == typeof(DateTime))
            {
                sb.Append('"');
                sb.Append(((DateTime)val).ToString("yyyy-MM-ddThh:mm:ss.fffZ"));
                sb.Append('"');
            }
            else if (!(val is string) && isArray)
            {

                var array = (IEnumerable)val;
                var arrstring = string.Join(" OR ",
                    array.Cast<object>().Select(x => string.Format("\"" + format + "\"", x)).ToArray());
                sb.AppendFormat(": ({0})", arrstring);

            }
            else
            {
                if (val is string)
                {

                    if (IsMultiList)
                    {
                        sb.Append(string.Format("({0})", val));
                    }
                    else
                    {
                        sb.Append(val.ToString().Replace(" ", "\\ "));
                    }

                }
                else
                {
                    if (!string.IsNullOrEmpty(formatValue))
                    {
                        sb.AppendFormat(format, val);
                    }
                    else
                    {
                        sb.Append(val);

                    }
                }
            }
            formatValue = null;
        }

        protected override MemberAssignment VisitMemberAssignment(MemberAssignment node)
        {
            return base.VisitMemberAssignment(node);
        }

        string formatValue = null;
#if NET35
        protected override Expression VisitMemberAccess(MemberExpression m)
#else
        protected override Expression VisitMember(MemberExpression m)
#endif
        {
            if (m.Expression != null && m.Expression.NodeType == ExpressionType.Parameter)
            {

                var fieldName = GetFieldName(m.Member, out formatValue);

                sb.Append(_isNotEqual ? string.Format("-{0}", fieldName) : fieldName);
                return m;
            }
            if (m.Expression != null)
            {
                if (m.Expression.NodeType == ExpressionType.Constant)
                {
                    var ce = (ConstantExpression)m.Expression;
                    if (!string.IsNullOrEmpty(formatValue))
                    {
                        sb.AppendFormat("{0:" + formatValue + "}", ce.Value);
                    }
                    else
                    {
                        sb.Append(ce.Value);
                    }
                    formatValue = null;
                }
                else if (m.Expression.NodeType == ExpressionType.MemberAccess)
                {

                    var ce = (MemberExpression)m.Expression;
                    var joinAttr = ce.Member.GetCustomAttribute<LinqToSolrForeignKeyAttribute>();

                    if (joinAttr != null)
                    {
                        var fieldName = GetFieldName(m.Member, out formatValue);
                        var topType = ce.Expression as ParameterExpression;
                        if (topType != null)
                        {
                            var joiner = new LinqToSolrJoiner(ce.Member.Name, topType.Type);
                            var joinstr = string.Format("!join from={0} to={1} fromIndex={2}", joiner.FieldKey, joiner.ForeignKey, query.Provider.Service.Configuration.GetIndex(joiner.PropertyRealType));
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
