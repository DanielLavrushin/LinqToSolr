using LinqToSolr.Attributes;
using LinqToSolr.Extensions;
using System;
using System.Collections;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Xml;

namespace LinqToSolr.Expressions
{

    internal class ExpressionTranslator<TObject> : ExpressionVisitor
    {
        Expression _expression { get; }
        StringBuilder q { get; set; }
        TranslatedQuery _queryResult;
        internal ExpressionTranslator(Expression expression)
        {
            _expression = expression;

        }
        public TranslatedQuery Translate(Expression expression, TranslatedQuery queryResult)
        {
            _queryResult = queryResult;
            q = new StringBuilder();
            Visit(Evaluator.PartialEval(BooleanVisitor.Process(expression)));
            var result = q.ToString();
            if (!string.IsNullOrEmpty(result))
            {
                _queryResult.Filters.Add(q.ToString());
            }
            return queryResult;
        }
        protected override Expression VisitMethodCall(MethodCallExpression node)
        {
            if (node.Method.Name == nameof(Enumerable.Where) || node.Method.Name == nameof(Enumerable.FirstOrDefault) || node.Method.Name == nameof(Enumerable.First))
            {
                var lambda = (LambdaExpression)StripQuotes(node.Arguments[1]);

                var solrQueryTranslator = new ExpressionTranslator<TObject>(lambda);
                var fq = solrQueryTranslator.Translate(lambda, _queryResult);
                Visit(node.Arguments[0]);
                return node;
            }

            if (node.Method.Name == nameof(string.Contains))
            {
                if (node.Object != null && node.Object.Type == typeof(string))
                {
                    Visit(node.Object);
                    q.Append(":");
                    q.Append("*");
                    Visit(node.Arguments[0]);
                    q.Append("*");
                    return node;
                }

                bool isCollection = node.Method.DeclaringType == typeof(Enumerable) || typeof(IEnumerable).IsAssignableFrom(node.Method.DeclaringType);
                bool isStatic = node.Method.IsStatic && node.Object == null;

                if (isCollection)
                {
                    var left = isStatic ? node.Arguments[1] : node.Object;
                    var right = isStatic ? node.Arguments[0] : node.Arguments[1];

                    if ((node.Arguments[0] as MemberExpression)?.Expression.NodeType == ExpressionType.Parameter ||
                        node.Arguments[0].NodeType == ExpressionType.Parameter)
                    {
                        left = node.Arguments[0];
                        right = isStatic ? node.Arguments[1] : node.Object;
                    }

                    Visit(left);
                    q.Append(":");
                    Visit(right);
                    return node;
                }
            }

            if (node.Method.Name == nameof(string.StartsWith))
            {
                Visit(node.Object);
                q.Append(":");
                Visit(node.Arguments[0]);
                q.Append("*");
                return node;
            }

            if (node.Method.Name == nameof(string.EndsWith))
            {
                Visit(node.Object);
                q.Append(":");
                q.Append("*");
                Visit(node.Arguments[0]);
                return node;
            }

            if (node.Method.Name == nameof(Enumerable.Take))
            {
                _queryResult.Take = EvalConstant<int>(node.Arguments[1]);
                return Visit(node.Arguments[0]);
            }

            if (node.Method.Name == nameof(Enumerable.Skip))
            {
                _queryResult.Skip = EvalConstant<int>(node.Arguments[1]);
                return Visit(node.Arguments[0]);
            }

            if (node.Method.Name == nameof(Enumerable.OrderBy) || node.Method.Name == nameof(Enumerable.ThenBy))
            {
                _queryResult.AddSorting(StripQuotes(node.Arguments[1]), SortingDirection.ASC);
                return Visit(node.Arguments[0]);
            }

            if (node.Method.Name == nameof(Enumerable.OrderByDescending) || node.Method.Name == nameof(Enumerable.ThenByDescending))
            {
                _queryResult.AddSorting(StripQuotes(node.Arguments[1]), SortingDirection.DESC);
                return Visit(node.Arguments[0]);
            }

            if (node.Method.Name == nameof(Enumerable.Select))
            {
                _queryResult.AddSelectField(StripQuotes(node.Arguments[1]));
                return Visit(node.Arguments[0]);
            }

            if (node.Method.Name == nameof(Enumerable.GroupBy))
            {
                _queryResult.AddGrouping(StripQuotes(node.Arguments[1]));
                return Visit(node.Arguments[0]);
            }

            throw new NotSupportedException($"The method '{node.Method.Name}' is not supported");
        }
        protected override Expression VisitBinary(BinaryExpression node)
        {
            var isGroup = node.NodeType == ExpressionType.AndAlso || node.NodeType == ExpressionType.OrElse;

            if (isGroup)
            {
                q.Append("(");
            }

            switch (node.NodeType)
            {
                case ExpressionType.And:
                case ExpressionType.AndAlso:
                    Visit(node.Left);
                    q.Append(" AND ");
                    Visit(node.Right);
                    break;
                case ExpressionType.Or:
                case ExpressionType.OrElse:
                    Visit(node.Left);
                    q.Append(" OR ");
                    Visit(node.Right);
                    break;
                case ExpressionType.NotEqual:
                    HandleNotEqual(node);
                    break;
                case ExpressionType.Equal:
                    //in case if the right side is null , we should use the -field:[* TO *] syntax
                    if (node.Right is ConstantExpression constRight && constRight.Value == null)
                    {
                        q.Append("-");
                    }
                    Visit(node.Left);
                    q.Append(":");
                    Visit(node.Right);
                    break;
                case ExpressionType.GreaterThan:
                    Visit(node.Left);
                    q.Append(":{");
                    Visit(node.Right);
                    q.Append(" TO *]");
                    break;
                case ExpressionType.LessThan:
                    Visit(node.Left);
                    q.Append(":[* TO ");
                    Visit(node.Right);
                    q.Append("}");
                    break;
                case ExpressionType.GreaterThanOrEqual:
                    Visit(node.Left);
                    q.Append(":[");
                    Visit(node.Right);
                    q.Append(" TO *]");
                    break;
                case ExpressionType.LessThanOrEqual:
                    Visit(node.Left);
                    q.Append(":[* TO ");
                    Visit(node.Right);
                    q.Append("}");
                    break;
                default:
                    throw new NotSupportedException($"The binary operator '{node.NodeType}' is not supported");
            }

            if (isGroup)
            {
                q.Append(")");
            }
            return node;
        }
        protected override Expression VisitConstant(ConstantExpression node)
        {
            var value = node.Value;
            if (value == null)
            {
                q.Append(@"[* TO *]");
                return node;
            }

            if (value is IQueryable qvalue)
            {
                if (qvalue.Expression.NodeType == ExpressionType.Call)
                {
                    Visit(qvalue.Expression);
                }
                return null;
            }
            var valueType = value.GetType();
            var isCollection = valueType == typeof(Enumerable) || typeof(IEnumerable).IsAssignableFrom(valueType);
            if (value is string)
            {
                q.Append(value?.ToString().Replace(" ", @"\ "));
            }
            else if (value is bool)
            {
                q.Append(value.ToString().ToLower());
            }
            else if (value is DateTime)
            {
                q.Append(((DateTime)value).ToString("yyyy-MM-ddTHH:mm:ss.fffZ", CultureInfo.InvariantCulture));
            }
            else if (isCollection)
            {
                var array = (IEnumerable)value;
                bool isFirst = true;
                q.Append("(");
                foreach (var item in array)
                {
                    if (!isFirst)
                    {
                        q.Append(" ");
                    }
                    Visit(Expression.Constant(item));
                    isFirst = false;
                }
                q.Append(")");
            }
            else
            {
                q.Append(value);
            }
            return node;
        }

        protected override Expression VisitMember(MemberExpression node)
        {
            if (node.Expression is ConstantExpression constantExpression)
            {
                var container = constantExpression.Value;
                var value = node.Member is FieldInfo field ? field.GetValue(container) : (node.Member as PropertyInfo).GetValue(node.Member, null);
                VisitConstant(Expression.Constant(value));
                return node;
            }
            else
            {
                q.Append(ExtractFieldName(node.Member));
            }

            return base.VisitMember(node);
        }
        protected override Expression VisitUnary(UnaryExpression node)
        {
            switch (node.NodeType)
            {
                case ExpressionType.Not:
                    q.Append("-");
                    break;
                case ExpressionType.Convert:
                case ExpressionType.Quote:
                    Visit(node.Operand);
                    break;
                default:
                    throw new NotSupportedException($"The unary operator '{node.Operand}' is not supported");
            }
            return node;
        }

        private void HandleNotEqual(BinaryExpression node)
        {
            var rightConstant = node.Right as ConstantExpression;
            var isRightNullConstant = rightConstant != null && rightConstant.Value == null;

            if (isRightNullConstant)
            {
                Visit(node.Left);
                q.Append(":[* TO *]");
            }
            else
            {
                var isRightBoolConstant = rightConstant != null && rightConstant.Type == typeof(bool);
                var rightValue = isRightBoolConstant ? (bool)rightConstant.Value : false;

                if (isRightBoolConstant && node.Left is MethodCallExpression methodCall)
                {
                    if (rightValue == true)
                    {
                        q.Append("-");
                        VisitMethodCall(methodCall);
                    }
                    else
                    {
                        Visit(methodCall.Object);
                        VisitMethodCall(methodCall);
                    }
                }
                else if (!isRightBoolConstant)
                {
                    q.Append("-");
                    Visit(node.Left);
                    q.Append(":");
                    Visit(node.Right);
                }
                else
                {
                    Visit(node.Left);
                    q.Append(rightValue ? ":false" : ":true");
                }
            }
        }

        internal static string ExtractFieldName(MemberInfo member)
        {
            return LinqToSolrFieldAttribute.GetFieldName(member);
        }

        private static Expression StripQuotes(Expression e)
        {
            while (e.NodeType == ExpressionType.Quote)
            {
                e = ((UnaryExpression)e).Operand;
            }
            return e;
        }

        T EvalConstant<T>(Expression node)
        {
            var constantExpression = node as ConstantExpression;
            if (constantExpression != null)
            {
                return (T)constantExpression.Value;
            }
            return default(T);
        }
    }
}
