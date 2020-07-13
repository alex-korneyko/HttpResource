using System;
using System.IO;
using System.Net;
using System.Text.Json;

namespace HttpResource
{
    class RequestHandler<T> where T : class
    {
        public string BearerToken { get; set; }
        
        private readonly ResponseHandler<T> _responseHandler;
        private readonly string _url;
        private readonly object _requestObject;

        public RequestHandler(string url, object requestObject, ResponseHandler<T> responseHandler)
        {
            _responseHandler = responseHandler;
            _url = url;
            _requestObject = requestObject;
        }

//----------------------------------------------------------------------------------------------------------------------
        public void GetRequest()
        {
            var webRequest = (HttpWebRequest)WebRequest.Create(_url);
            webRequest.ContentType = "application/json";
            webRequest.Method = "GET";
            webRequest.Headers.Add(HttpRequestHeader.Authorization, $"Bearer {BearerToken}");
            
            HttpWebResponse webResponse;
            try
            {
                webResponse = (HttpWebResponse) webRequest.GetResponse();
            }
            catch (WebException e)
            {
                var httpStatusCode = ((HttpWebResponse)e.Response).StatusCode;
                _responseHandler.Invoke(null, httpStatusCode);
                return;
            }

            var resultObject = GetResultObject(webResponse);
            
            _responseHandler.Invoke(resultObject, webResponse.StatusCode);
        }

//----------------------------------------------------------------------------------------------------------------------
        public void PostRequest()
        {
            var webRequest = (HttpWebRequest)WebRequest.Create(_url);
            webRequest.ContentType = "application/json";
            webRequest.Method = "POST";
            webRequest.Headers.Add(HttpRequestHeader.Authorization, $"Bearer {BearerToken}");

            try
            {
                using (var streamWriter = new StreamWriter(webRequest.GetRequestStream()))
                {
                    var json = JsonSerializer.Serialize(_requestObject);
                    streamWriter.Write(json);
                }
            }
            catch (WebException e)
            {
                _responseHandler.Invoke(null, ((HttpWebResponse)e.Response).StatusCode);
                return;
            }
            
            HttpWebResponse webResponse;
            try
            {
                webResponse = (HttpWebResponse) webRequest.GetResponse();
            }
            catch (WebException e)
            {
                var httpStatusCode = ((HttpWebResponse)e.Response).StatusCode;
                _responseHandler.Invoke(null, httpStatusCode);
                return;
            }

            var resultObject = GetResultObject(webResponse);
            
            _responseHandler.Invoke(resultObject, webResponse.StatusCode);
        }

//----------------------------------------------------------------------------------------------------------------------
        private static T GetResultObject(WebResponse webResponse)
        {
            T resultObject;
            
            using (var streamReader =
                new StreamReader(webResponse.GetResponseStream() ?? throw new InvalidOperationException()))
            {
                var stringResponse = streamReader.ReadToEnd();
                var options = new JsonSerializerOptions {PropertyNameCaseInsensitive = true};

                resultObject = stringResponse != "" ? JsonSerializer.Deserialize<T>(stringResponse, options) : null;
            }

            return resultObject;
        }
    }
}