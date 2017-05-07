using System;
using System.Collections;
using System.Linq;
using System.Linq.Expressions;

using LinqToSolr.Expressions;
using LinqToSolr.Services;

namespace LinqToSolr.Query
{
    public class LinqToSolrProvider: IQueryProvider
    {
        internal Type ElementType;
        internal ILinqToSolrService Service;
        internal bool IsEnumerable;
        public LinqToSolrProvider(ILinqToSolrService service)
        {
            Service = service;
            ElementType = Service.ElementType;
        }
        public IQueryable CreateQuery(Expression expression)
        {
            ElementType = TypeSystem.GetElementType(expression.Type);
            try
            {
                return (IQueryable)Activator.CreateInstance(typeof(LinqToSolrQueriable<>).MakeGenericType(ElementType), new object[] { this, expression });
            }
            catch (System.Reflection.TargetInvocationException ex)
            {
                throw ex.InnerException;
            }
        }

        public IQueryable<TElement> CreateQuery<TElement>(Expression expression)
        {
            return new LinqToSolrQueriable<TElement>(this, expression);
        }



        public object Execute(Expression expression)
        {
            var query = GetSolrQuery(expression).Query(ElementType);

            return IsEnumerable ? query : ((IEnumerable)query).Cast<object>().FirstOrDefault();
        }


        public TResult Execute<TResult>(Expression expression)
        {
            IsEnumerable = (typeof(TResult).Name == "IEnumerable`1");

            var result = Execute(expression);
            return (TResult)result;
        }

        internal ILinqToSolrService GetSolrQuery(Expression expression)
        {
            var elementType = TypeSystem.GetElementType(expression.Type);
            Service.ElementType = elementType;
            var qt = new LinqToSolrQueryTranslator(Service);
            expression = Evaluator.PartialEval(expression);

            Service.CurrentQuery.FilterUrl = qt.Translate(BooleanVisitor.Process(expression));


            return Service;
        }

    }
}
