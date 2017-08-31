using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Net;
using System.Reflection;
using LinqToSolr.Converters;
using LinqToSolr.Data;
using LinqToSolr.Expressions;
using LinqToSolr.Query;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace LinqToSolr.Services
{
    internal class SolrErrorResponse
    {

    }
    public class LinqToSolrService: ILinqToSolrService
    {
        public LinqToSolrRequestConfiguration Configuration { get; set; }
        public ILinqToSolrResponse LastResponse { get; set; }
        public Type ElementType { get; set; }
        public string CurrentFilterUrl { get; set; }
        protected SolrWebClient Client;
        public LinqToSolrQuery CurrentQuery { get; set; }


        public LinqToSolrService()
        {
            Configuration = LinqToSolrRequestConfiguration._instance;

            if (Configuration.Take <= 0)
            {
                Configuration.Take = 10;
            }
            if (Configuration == null || string.IsNullOrEmpty(Configuration.EndPoint))
                throw new Exception("Solr Endpoint was not provided. Specify Solr server in SolrRequestConfiguration.");
            Client = new SolrWebClient(Configuration.EndPoint);
        }

        public LinqToSolrService(LinqToSolrRequestConfiguration configuration)
        {
            Configuration = configuration;
            if (Configuration.Take <= 0)
            {
                Configuration.Take = 10;
            }
            if (string.IsNullOrEmpty(Configuration.EndPoint))
                throw new Exception("Solr Endpoint was not provided. Specify Solr server in SolrRequestConfiguration.");
            Client = new SolrWebClient(Configuration.EndPoint);

        }

        public ICollection<T> LastDocuments<T>()
        {
            return LastResponse?.Body?.Documents?.Cast<T>().ToList();
        }


        public LinqToSolrQueriable<T> AsQueryable<T>()
        {
            CurrentQuery = new LinqToSolrQuery() { CurrentType = typeof(T) };
            return new LinqToSolrQueriable<T>(this);
        }

        private SolrWebRequest PrepareQueryRequest(string index)
        {


            string path = string.Format("/{1}/{0}/select", index, string.IsNullOrEmpty( Configuration.SolrPath) ? "solr": Configuration.SolrPath);
            var request = new SolrWebRequest(path);


            request.AddParameter("q", "*");
            request.AddParameter("wt", "json");
            request.AddParameter("indent", "true");
            request.AddParameter("rows", Configuration.Take.ToString());
            request.AddParameter("start", Configuration.Start.ToString());

            if (CurrentQuery.JoinFields.Any())
            {
                foreach (var joiner in CurrentQuery.JoinFields)
                {
                    var joinCore = Configuration.GetIndex(joiner.PropertyRealType);
                    var joinstr = string.Format("from={0} to={1} fromIndex={2}", joiner.ForeignKey, joiner.FieldKey, joinCore);
                    request.AddParameter("q", "{!join " + joinstr + "}");
                }

            }

            if (CurrentQuery.IsGroupEnabled)
            {
                request.AddParameter("group", "true");
                request.AddParameter("group.limit", Configuration.Take.ToString());
                request.AddParameter("group.offset", Configuration.Start.ToString());
                foreach (var groupField in CurrentQuery.GroupFields)
                {
                    request.AddParameter("group.field", groupField);
                }

            }


            if (CurrentQuery.Filters.Any())
            {
                foreach (var filter in CurrentQuery.Filters)
                {
                    request.AddParameter("fq", string.Format("{0}: ({1})", filter.Name,
                        string.Join(" OR ", filter.Values.Select(x => string.Format("\"{0}\"", x)).ToArray()
                        )));
                }
            }


            if (!string.IsNullOrEmpty(CurrentQuery.FilterUrl))
            {
                foreach (var fstring in CurrentQuery.FilterUrl.Split(new[] { "&fq=" }, StringSplitOptions.None))
                {
                    if (!string.IsNullOrEmpty(fstring))
                    {
                        request.AddParameter("fq", fstring);
                    }
                }
            }

            if (CurrentQuery.Sortings.Any())
            {
                request.AddParameter("sort", string.Join(", ", CurrentQuery.Sortings.Select(x =>
                        string.Format("{0} {1}", x.Name, x.Order == SolrSortTypes.Desc ? "DESC" : "ASC")).ToArray()));
            }


            if (CurrentQuery.Facets.Any())
            {
                request.AddParameter("facet", "true");
                request.AddParameter("facet.mincount", "1");

                if (Configuration.FacetsLimit > 0)
                {
                    request.AddParameter("facet.limit", Configuration.FacetsLimit.ToString());
                }

                var ignoredFacets = CurrentQuery.FacetsToIgnore.Select(x => x.SolrName).ToList();
                foreach (var facet in CurrentQuery.Facets)
                {
                    request.AddParameter("facet.field", ignoredFacets.Any(x => x == facet.SolrName)
                        ? string.Format("{{!ex={0}}}{0}", facet.SolrName)
                        : facet.SolrName);
                }
            }

            if (CurrentQuery.Select != null)
            {
                request.AddParameter("fl", CurrentQuery.Select.GetSelectFields());

            }


            return request;
        }

        private SolrWebRequest PrepareUpdateOrDeleteRequest<T>(T[] documentsToUpdate, object[] deleteDocIds, string deleteByQuery, bool softCommit = false)
        {

            var index = Configuration.GetIndex(typeof(T));

            if (string.IsNullOrEmpty(index))
            {
                throw new ArgumentNullException(nameof(index),
                    string.Format(
                        "The type '{0}' is not assigned for any Solr Index. Register this type in a service configuration (SolrRequestConfiguration.MapIndexFor)",
                       typeof(T).Name));
            }

            string path = string.Format("/{1}/{0}/update", index, string.IsNullOrEmpty(Configuration.SolrPath) ? "solr" : Configuration.SolrPath);
            var request = new SolrWebRequest(path, SolrWebMethod.POST);

            var updateDocs = JsonConvert.SerializeObject(documentsToUpdate,
                Newtonsoft.Json.Formatting.None,
                new JsonSerializerSettings
                {
                    NullValueHandling = NullValueHandling.Ignore
                });

            request.AddParameter("wt", "json");
            request.AddParameter("commit", "true");
            if (softCommit)
            {
                request.AddParameter("softCommit", "true");

            }
            if (documentsToUpdate != null && documentsToUpdate.Any())
            {
                request.Body = updateDocs;
            }
            else if (deleteDocIds != null && deleteDocIds.Any())
            {
                request.Body = JsonConvert.SerializeObject(new { delete = deleteDocIds });
            }
            else if (!string.IsNullOrEmpty(deleteByQuery))
            {
                request.Body = JsonConvert.SerializeObject(new { delete = new { query = deleteByQuery } });
            }




            return request;
        }

        private void PerformUpdate<T>(T[] documentsToUpdate, bool softCommit = false)
        {
            var request = PrepareUpdateOrDeleteRequest(documentsToUpdate, null, null, softCommit);
            FinalizeResponse(request);


        }

        private void PerformDelete<T>(string query, bool softCommit = false)
        {
            var request = PrepareUpdateOrDeleteRequest<T>(null, null, query, softCommit);
            FinalizeResponse(request);


        }

        private void PerformDelete<T>(object[] documentIds, bool softCommit = false)
        {

            var request = PrepareUpdateOrDeleteRequest<T>(null, documentIds, null, softCommit);
            FinalizeResponse(request);
        }


        private void FinalizeResponse(SolrWebRequest request)
        {
            var responce = Client.Execute(request);
            if (responce.StatusCode == HttpStatusCode.OK || responce.StatusCode == HttpStatusCode.NoContent)
            {
                LastResponse = new LinqToSolrResponse { LastServiceUri = responce.ResponseUri };
                return;
            }

            if (!string.IsNullOrEmpty(responce.Content))
            {
                LastResponse = JsonConvert.DeserializeObject<LinqToSolrResponse>(responce.Content);
                LastResponse.LastServiceUri = responce.ResponseUri;
                LastResponse.Content = responce.Content;
            }

            if (LastResponse.Error != null)
                throw new Exception("Oops! SOLR Says: " + LastResponse.Error.Message);
        }

        public void AddOrUpdate<T>(T[] document, bool softCommit = false)
        {
            if (document == null)
                throw new ArgumentNullException(nameof(document));

            PerformUpdate(document, softCommit);
        }
        public void AddOrUpdate<T>(T document, bool softCommit = false)
        {
            AddOrUpdate<T>(new[] { document }, softCommit);
        }

        public void Delete<T>(object documentId, bool softCommit = false)
        {
            Delete<T>(new object[] { documentId }, softCommit);
        }

        public void Delete<T>(object[] documentId, bool softCommit = false)
        {
            if (documentId == null)
                throw new ArgumentNullException(nameof(documentId));

            PerformDelete<T>(documentId, softCommit);
        }
        public void Delete<T>(Expression<Func<T, bool>> query, bool softCommit = false)
        {
            if (query == null)
                throw new ArgumentNullException(nameof(query));

            var translator = new LinqToSolrQueryTranslator(this, typeof(T));
            var q = Evaluator.PartialEval(query);
            var queryToStr = translator.Translate(BooleanVisitor.Process(q));
            PerformDelete<T>(queryToStr, softCommit);
        }

        public ICollection<T> Query<T>(LinqToSolrQuery query = null)
        {
            return Query(typeof(T), query) as ICollection<T>;
        }

        private void ErrorHandler(object sender, Newtonsoft.Json.Serialization.ErrorEventArgs errorEventArgs)
        {
            errorEventArgs.ErrorContext.Handled = true;
        }

        public object Query(Type elementType, LinqToSolrQuery query = null)
        {
            if (query != null)
            {
                CurrentQuery = query;
            }
            var index = Configuration.GetIndex(elementType);

            if (string.IsNullOrEmpty(index))
            {
                throw new ArgumentNullException(nameof(index),
                    string.Format(
                        "The type '{0}' is not assigned for any Solr Index. Register this type in a service configuration (SolrRequestConfiguration.MapIndexFor)",
                        ElementType.Name));
            }

            if (!string.IsNullOrEmpty(Configuration.SolrLogin) && !string.IsNullOrEmpty(Configuration.SolrPassword))
            {
                //   Client.Authenticator = new HttpBasicAuthenticator(Configuration.SolrLogin, Configuration.SolrPassword);
            }

            var response = Client.Execute(PrepareQueryRequest(index));
            LastResponse = new LinqToSolrResponse();
            LastResponse.LastServiceUri = response.ResponseUri;

            if (response.StatusCode == HttpStatusCode.NotFound)
            {
                throw new Exception("Server returned 404 - Not Found. Check if solr url is correct or index name was correctly provided.");
            }

            if (response.StatusCode == HttpStatusCode.Unauthorized)
            {
                throw new UnauthorizedAccessException(
                    "Solr is protected by password. Setup login and passowrd in the configuration class.");
            }

            if (response.ErrorException != null)
            {
                throw response.ErrorException;
            }

            LastResponse = JsonConvert.DeserializeObject<LinqToSolrResponse>(response.Content, new LinqToSolrRawJsonConverter());
           
            LastResponse.LastServiceUri = response.ResponseUri;
            if (LastResponse.Header.Status == 0)
            {
                if (LastResponse.Body != null)
                {

                    LastResponse.FoundDocuments = (int)LastResponse.Body?.Count;

                    var listMethod =
                        typeof(List<>).MakeGenericType(CurrentQuery.Select != null && !CurrentQuery.Select.IsSingleField
                            ? CurrentQuery.Select.Type
                            : elementType);

                    var genList = JsonConvert.DeserializeObject(JsonConvert.SerializeObject(LastResponse.Body.Documents), listMethod, 
                        new JsonSerializerSettings { Error = ErrorHandler, Converters = new List<JsonConverter>{ new LinqToSolrRawJsonConverter() } }) as IEnumerable;
                    LastResponse.Body.Documents = genList.Cast<object>().ToList();



                    if (LastResponse.Facets != null)
                    {

                        foreach (var facet in CurrentQuery.Facets)
                        {
                            var content = JsonConvert.DeserializeObject<JObject>(response.Content)["facet_counts"];
                            var groups = content["facet_fields"][facet.SolrName] as JArray;

                            LastResponse.Facets.Add(facet.SolrName,
                                groups.Where((x, i) => i % 2 == 0).Select(x => ((JValue)x).Value).ToArray());
                        }
                    }

                    if (CurrentQuery.Select?.IsSingleField == true)
                    {
                        var fieldDelegate = ((LambdaExpression)CurrentQuery.Select.Expression).Compile();

#if PORTABLE || NETCORE
                        
                        var selectMethod = typeof(Enumerable).GetRuntimeMethods().First(m => m.Name == "Select" && m.GetParameters().Count() == 2);
#else
                        var selectMethod = typeof(Enumerable).GetMethods(BindingFlags.Static | BindingFlags.Public).First(m => m.Name == "Select" && m.GetParameters().Count() == 2);
#endif
                        var fieldList = selectMethod.MakeGenericMethod(elementType, CurrentQuery.Select.Type)
                            .Invoke(genList, new object[] { genList, fieldDelegate });
                        CurrentQuery = null;
                        return fieldList;
                    }

                    CurrentQuery = null;
                    return genList;
                }


                if (CurrentQuery.IsGroupEnabled)
                {
#if PORTABLE || NETCORE
                    var args =ElementType.GetTypeInfo().IsGenericTypeDefinition
                        ? ElementType.GetTypeInfo().GenericTypeParameters
                        : ElementType.GetTypeInfo().GenericTypeArguments;
                    var _keyType = args[0];
                    var _valueType = args[1];

#else
                    var _keyType = ElementType.GetGenericArguments()[0];
                    var _valueType = ElementType.GetGenericArguments()[1];

#endif

                    var solrConverterType = typeof(LinqToSolrGroupingResponseConverter<,>).MakeGenericType(_keyType, _valueType);
                    var converterInstance = Activator.CreateInstance(solrConverterType);

                    var groupResult = JsonConvert.DeserializeObject(response.Content, solrConverterType,
                      converterInstance as JsonConverter);
                    return groupResult;
                }


                CurrentQuery = null;
                return null;
            }
            throw new Exception(LastResponse.Error.Message);
        }



    }
}
