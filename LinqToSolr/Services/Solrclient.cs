using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;


namespace LinqToSolr.Services
{
    public enum SolrWebMethod
    {
        GET,
        POST,
        DELETE
    }

    public class SolrWebParameter
    {
        public string Name { get; set; }
        public object Value { get; set; }

        public SolrWebParameter(string name, object value)
        {
            Name = name;
            Value = value;
        }
    }

    public class SolrWebRequest
    {

        public SolrWebMethod Method { get; set; }
        public string ActionUrl { get; set; }
        public string Body { get; set; }
        public ICollection<SolrWebParameter> Parameters { get; set; }

        public SolrWebRequest(string action, SolrWebMethod method = SolrWebMethod.GET)
        {
            ActionUrl = action;
            Method = method;
            Parameters = new List<SolrWebParameter>();
        }

        public void AddParameter(string name, object value)
        {
            Parameters.Add(new SolrWebParameter(name, value));
        }

    }

    public class SolrWebResponse
    {
        public string Content { get; set; }
        public Uri ResponseUri { get; set; }

        public HttpStatusCode StatusCode { get; set; }
        public Exception ErrorException { get; set; }
    }

    public class SolrWebClient
    {
        private string _endpoint;
        private SolrWebRequest CurrentRequest;
        public SolrWebClient(string endPoint)
        {
            _endpoint = endPoint;
        }

#if NET40 || PORTABLE40 || PORTABLE

        private void GetRequestStreamCallback(IAsyncResult asynchronousResult)
        {
            HttpWebRequest request = (HttpWebRequest)asynchronousResult.AsyncState;

            var postStream = request.EndGetRequestStream(asynchronousResult);

            byte[] byteArray = Encoding.UTF8.GetBytes(CurrentRequest.Body);

            postStream.Write(byteArray, 0, CurrentRequest.Body.Length);
            postStream.Flush();

            var result = request.BeginGetResponse(new AsyncCallback(GetResponseCallback), request);
        }

        private void GetResponseCallback(IAsyncResult asynchronousResult)
        {
            var request = (HttpWebRequest)asynchronousResult.AsyncState;
            var response = (HttpWebResponse)request.EndGetResponse(asynchronousResult);
            var streamResponse = response.GetResponseStream();
            var streamRead = new System.IO.StreamReader(streamResponse);
            var responseString = streamRead.ReadToEnd();
            streamResponse.Flush();
        }

#endif
        public SolrWebResponse Execute(SolrWebRequest request)
        {

            var paramStr =
                string.Join("&", request.Parameters.Select(x => string.Format("{0}={1}", x.Name, x.Value)).ToArray());
            _endpoint = _endpoint.EndsWith("/") ? _endpoint.TrimEnd('/') : _endpoint;
            request.ActionUrl = request.ActionUrl.StartsWith("/") ? request.ActionUrl.TrimStart('/') : request.ActionUrl;
            paramStr = string.Format("{0}/{1}?{2}", _endpoint, request.ActionUrl, paramStr.Substring(0, paramStr.Length));
            var response = new SolrWebResponse();
            response.ResponseUri = new Uri(paramStr);

#if NET40 || PORTABLE40 || PORTABLE


            HttpWebRequest webRequest = (HttpWebRequest)WebRequest.Create(paramStr);


            if (request.Method == SolrWebMethod.GET)
            {
                using (var webResponse =
                    (HttpWebResponse)(System.Threading.Tasks.Task<WebResponse>.Factory.FromAsync(
                        webRequest.BeginGetResponse, webRequest.EndGetResponse, null)).Result)
                {
                    using (System.IO.Stream stream = webResponse.GetResponseStream())
                    {
                        var reader = new System.IO.StreamReader(stream, Encoding.UTF8);
                        response.Content = reader.ReadToEnd();
                        response.StatusCode = webResponse.StatusCode;
                    }
                }
            }
            else
            {
                webRequest.Method = "POST";
                webRequest.ContentType = "application/json";
                CurrentRequest = request;
                var result = (HttpWebRequest)webRequest.BeginGetRequestStream(new AsyncCallback(GetRequestStreamCallback), webRequest).AsyncState;
                return response;


            }

#elif NET35

            var webRequest = (HttpWebRequest)WebRequest.Create(paramStr);


            if (request.Method == SolrWebMethod.GET)
            {
                using (var webResponse = (HttpWebResponse)webRequest.GetResponse())
                {
                    var encoding = Encoding.GetEncoding(webResponse.CharacterSet);

                    using (var responseStream = webResponse.GetResponseStream())
                    {
                        using (var reader = new System.IO.StreamReader(responseStream, encoding))
                        {
                            response.Content = reader.ReadToEnd();
                            response.StatusCode = webResponse.StatusCode;
                        }
                    }
                }
            }
            else
            {
                webRequest.Method = "POST";
                webRequest.ContentType = "application/x-www-form-urlencoded";
                webRequest.ContentLength = request.Body.Length;
                var data = Encoding.UTF8.GetBytes(request.Body);
                using (var stream = webRequest.GetRequestStream())
                {
                    stream.Write(data, 0, data.Length);
                }

                var webResponse = (HttpWebResponse)webRequest.GetResponse();
                response.Content = new System.IO.StreamReader(webResponse.GetResponseStream()).ReadToEnd();
                response.StatusCode = webResponse.StatusCode;
            }

#else
            var client = new System.Net.Http.HttpClient();
            client.DefaultRequestHeaders.Accept.Add(
                new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));

            if (request.Method == SolrWebMethod.GET)
            {
                var webResponse = client.GetAsync(paramStr).Result;
                response.Content = webResponse.Content.ReadAsStringAsync().Result;
                response.StatusCode = webResponse.StatusCode;
            }
            else
            {
                var content = new System.Net.Http.StringContent(request.Body, Encoding.UTF8, "application/json");
                var result = client.PostAsync(paramStr, content).Result;
                var webResponse = client.GetAsync(paramStr).Result;
                response.Content = webResponse.Content.ReadAsStringAsync().Result;
                response.StatusCode = webResponse.StatusCode;
            }


#endif
            return response;
        }
    }
}
