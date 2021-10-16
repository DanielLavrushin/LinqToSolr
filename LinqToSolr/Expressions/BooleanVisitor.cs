using System.Linq.Expressions;

namespace LinqToSolr.Expressions
{
    internal class BooleanVisitor : ExpressionVisitor
    {
        public static Expression Process(Expression expression)
        {
            return new BooleanVisitor().Visit(expression);
        }

        int _bypass;



        protected override Expression VisitBinary(BinaryExpression node)
        {
            if (_bypass == 0 && node.Type == typeof(bool))
            {
                switch (node.NodeType)
                {
                    case ExpressionType.And: // bitwise & - different to &&
                    case ExpressionType.Or: // bitwise | - different to ||
                    case ExpressionType.Equal:
                    case ExpressionType.NotEqual:
                        _bypass++;
                        var result = base.VisitBinary(node);
                        _bypass--;
                        return result;
                }
            }
            return base.VisitBinary(node);
        }

        protected override Expression VisitUnary(UnaryExpression node)
        {
            if (_bypass == 0 && node.Type == typeof(bool))
            {
                switch (node.NodeType)
                {
                    case ExpressionType.Not:
                        _bypass++;
                        var result = Expression.NotEqual(
                            Visit(node.Operand),
                            Expression.Constant(true));
                        _bypass--;
                        return result;
                }
            }
            return base.VisitUnary(node);
        }


#if NET35
        protected override Expression VisitMemberAccess(MemberExpression node)
        {
            if (_bypass == 0 && node.Type == typeof(bool))
            {
                return Expression.Equal(base.VisitMemberAccess(node), Expression.Constant(true));
            }
            return base.VisitMemberAccess(node);
        }
#else
        protected override Expression VisitMember(MemberExpression node)
        {
            if (_bypass == 0 && node.Type == typeof(bool))
            {
                return Expression.Equal(base.VisitMember(node), Expression.Constant(true));
            }
            return base.VisitMember(node);
        }
#endif
    }
}