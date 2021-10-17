using System;
using System.Collections;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using LinqToSolr.Expressions;
using LinqToSolr.Services;
using LinqToSolr.Data;
using System.Collections.Generic;
using LinqToSolr.Interfaces;
using LinqToSolr.Helpers.Json;

namespace LinqToSolr.Query
{
    public interface ILinqToSolrProvider : IQueryProvider
    {
        ILinqToSolrService Service { get; }
        ICollection<TResult> ExecuteQuery<TResult>(LinqToSolrQueriable<TResult> quariable);

    }
    public class LinqToSolrProvider : ILinqToSolrProvider
    {
        public ILinqToSolrService Service { get; }
        internal bool IsEnumerable;
        IQueryable query;
        SolrWebClient Client;

        public LinqToSolrProvider(ILinqToSolrService service)
        {
            Service = service;
            Client = new SolrWebClient(Service.Configuration.EndPoint.ToString());
        }

        public IQueryable CreateQuery(Expression expression)
        {
            var elementType = TypeSystem.GetElementType(expression.Type);
            query = (IQueryable)Activator.CreateInstance(typeof(LinqToSolrQueriable<>).MakeGenericType(elementType), new object[] { this, expression });
            return query;
        }
        public IQueryable<TElement> CreateQuery<TElement>(Expression expression)
        {
            return CreateQuery(expression) as LinqToSolrQueriable<TElement>;
        }

        public object Execute(Expression expression)
        {
            var elementType = TypeSystem.GetElementType(expression.Type);
            var providerQuery = GetType().GetMethod("ExecuteQuery").MakeGenericMethod(new[] { elementType });
            var result = providerQuery.Invoke(this, new[] { query });
            return IsEnumerable ? result : ((IEnumerable)result).Cast<object>().FirstOrDefault();
        }


        public TResult Execute<TResult>(Expression expression)
        {
            IsEnumerable = typeof(TResult).Name == "IEnumerable`1";
            return (TResult)Execute(expression);
        }

        internal string GetSolrUrl(ILinqToSolrQuery query)
        {
            var qt = new LinqToSolrQueryTranslator(query);
            var expression = Evaluator.PartialEval(query.Expression, e => e.NodeType != ExpressionType.Parameter && !typeof(IQueryable).IsAssignableFrom(e.Type));
            return qt.Translate(BooleanVisitor.Process(expression));
        }

        internal SolrWebRequest PrepareQueryRequest<TResult>(ILinqToSolrQuery query)
        {
            var config = Service.Configuration;
            string path = string.Format("/{0}/select", config.GetIndex(typeof(TResult)));
            var request = new SolrWebRequest(path);

            request.AddParameter("q", "*");
            request.AddParameter("wt", "json");
            request.AddParameter("indent", "true");
            request.AddParameter("rows", (query.Take > 0 ? query.Take : config.Take).ToString());
            request.AddParameter("start", (query.Start > 0 ? query.Start : config.Start).ToString());

            if (query.JoinFields.Any())
            {
                foreach (var joiner in query.JoinFields)
                {
                    var joinCore = config.GetIndex(joiner.PropertyRealType);
                    var joinstr = string.Format("from={0} to={1} fromIndex={2}", joiner.ForeignKey, joiner.FieldKey, joinCore);
                    request.AddParameter("q", "{!join " + joinstr + "}");
                }
            }

            if (query.GroupFields.Any())
            {
                request.AddParameter("group", "true");
                request.AddParameter("group.limit", (query.Take > 0 ? query.Take : config.Take).ToString());
                request.AddParameter("group.offset", (query.Start > 0 ? query.Start : config.Start).ToString());
                foreach (var groupField in query.GroupFields)
                {
                    request.AddParameter("group.field", groupField);
                }
            }

            if (query.Filters.Any())
            {
                foreach (var filter in query.Filters)
                {
                    if (filter.Values == null)
                    {
                        request.AddParameter("fq", filter.Field);
                    }
                    else
                    {
                        request.AddParameter("fq", string.Format("{0}: ({1})", filter.Field,
                            string.Join(" OR ", filter.Values.Select(x => string.Format("\"{0}\"", x)).ToArray()
                            )));
                    }
                }
            }


            //if (!string.IsNullOrEmpty(query.FilterUrl))
            //{
            //    foreach (var fstring in query.FilterUrl.Split(new[] { "&fq=" }, StringSplitOptions.None))
            //    {
            //        if (!string.IsNullOrEmpty(fstring))
            //        {
            //            request.AddParameter("fq", fstring);
            //        }
            //    }
            //}

            if (query.Sortings.Any())
            {
                request.AddParameter("sort", string.Join(", ", query.Sortings.Select(x =>
                        string.Format("{0} {1}", x.Field, x.Order == SolrSortTypes.Desc ? "DESC" : "ASC")).ToArray()));
            }


            if (query.Facets.Any())
            {
                request.AddParameter("facet", "true");
                request.AddParameter("facet.mincount", "1");

                if (config.FacetsLimit > 0)
                {
                    request.AddParameter("facet.limit", config.FacetsLimit.ToString());
                }

                var ignoredFacets = query.FacetsToIgnore.Select(x => x.Field).ToList();
                foreach (var facet in query.Facets)
                {
                    request.AddParameter("facet.field", ignoredFacets.Any(x => x == facet.Field)
                        ? string.Format("{{!ex={0}}}{0}", facet.Field)
                        : facet.Field);
                }
            }

            if (query.Select != null)
            {
                request.AddParameter("fl", query.Select.GetSelectFields());
            }

            return request;
        }


        public virtual ICollection<TResult> ExecuteQuery<TResult>(LinqToSolrQueriable<TResult> quariable)
        {
            var solrQuery = quariable.Translate();
            var request = PrepareQueryRequest<TResult>(solrQuery);
            var response = Client.Execute(request);

            Service.LastResponseUrl = response.ResponseUri;
            var current = response.Content.FromJson<SolrResponse<TResult>>();

            return current.Response.Documents;
        }
    }
}
