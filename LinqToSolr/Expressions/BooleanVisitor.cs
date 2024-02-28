using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text;

namespace LinqToSolr.Expressions
{
    internal class BooleanVisitor : ExpressionVisitor
    {
        public static Expression Process(Expression expression)
        {
            return new BooleanVisitor().Visit(expression);
        }

        int bypass;
        protected override Expression VisitBinary(BinaryExpression node)
        {
            if (bypass == 0 && node.Type == typeof(bool))
            {
                switch (node.NodeType)
                {
                    case ExpressionType.And: // bitwise & - different to &&
                    case ExpressionType.Or: // bitwise | - different to ||
                    case ExpressionType.Equal:
                    case ExpressionType.NotEqual:
                        bypass++;
                        var result = base.VisitBinary(node);
                        bypass--;
                        return result;
                }
            }
            return base.VisitBinary(node);
        }
        protected override Expression VisitUnary(UnaryExpression node)
        {
            if (bypass == 0 && node.Type == typeof(bool))
            {
                switch (node.NodeType)
                {
                    case ExpressionType.Not:
                        bypass++;
                        var result = Expression.NotEqual(
                            base.Visit(node.Operand),
                            Expression.Constant(true));
                        bypass--;
                        return result;
                }
            }
            return base.VisitUnary(node);
        }
        protected override Expression VisitMember(MemberExpression node)
        {
            if (bypass == 0 && node.Type == typeof(bool))
            {
                return Expression.Equal(
                    base.VisitMember(node),
                    Expression.Constant(true));
            }
            return base.VisitMember(node);
        }
    }
}
