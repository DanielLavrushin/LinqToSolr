using LinqToSolr.Attributes;
using System;
using System.Collections.Generic;
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
        internal ExpressionTranslator(Expression expression)
        {
            _expression = expression;

        }
        public string Translate(Expression expression)
        {
            q = new StringBuilder();
            Visit(Evaluator.PartialEval(BooleanVisitor.Process(expression)));

            var queryurl = q.ToString();
            return queryurl;
        }
        protected override Expression VisitMethodCall(MethodCallExpression node)
        {
            if (node.Method.Name == nameof(Enumerable.Where) || node.Method.Name == nameof(Enumerable.FirstOrDefault) || node.Method.Name == nameof(Enumerable.First))
            {
                return Visit(node.Arguments.Last());
            }


            Visit(node.Object);
            q.Append(":");

            if (node.Method.Name == nameof(string.Contains))
            {
                q.Append("*");
                Visit(node.Arguments[0]);
                q.Append("*");
                return node;
            }

            if (node.Method.Name == nameof(string.StartsWith))
            {
                q.Append(" *");
                return Visit(node.Arguments[0]);
            }

            if (node.Method.Name == nameof(string.EndsWith))
            {
                Visit(node.Arguments[0]);
                q.Append("* ");
                return node;
            }
            throw new NotSupportedException(string.Format("The method '{0}' is not supported", node.Method.Name));
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
                case ExpressionType.Equal:
                case ExpressionType.NotEqual:
                    if (node.NodeType == ExpressionType.NotEqual)
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
                    throw new NotSupportedException(string.Format("The binary operator '{0}' is not supported", node.NodeType));
            }

            if (isGroup)
            {
                q.Append(")");
            }
            return node;
        }
        protected override Expression VisitConstant(ConstantExpression node)
        {
            q.Append(ExtractConstant(node.Value));
            return base.VisitConstant(node);
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
            }

            return base.VisitUnary(node);
        }
        protected override Expression VisitParameter(ParameterExpression node)
        {
            return base.VisitParameter(node);
        }
        internal string ExtractFieldName(MemberInfo member)
        {
            return LinqToSolrFieldAttribute.GetFieldName(member);
        }
        object ExtractConstant(object value)
        {
            if (value == null)
            {
                return "(*) AND *:*";
            }

            if (value is string)
            {
                return value?.ToString().Replace(" ", @"\ ");
            }
            else if (value is bool)
            {
                return value.ToString().ToLower();
            }
            else if (value is DateTime)
            {
                return ((DateTime)value).ToString("yyyy-MM-ddTHH:mm:ss.fff", CultureInfo.InvariantCulture) + "Z";
            }
            else
            {
                return value;
            }
        }

        private static Expression StripQuotes(Expression e)
        {
            while (e.NodeType == ExpressionType.Quote)
            {
                e = ((UnaryExpression)e).Operand;
            }
            return e;
        }
    }
}
