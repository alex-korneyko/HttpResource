using System.Net;
using System.Threading;

namespace HttpResource
{
    public delegate void ResponseHandler<T>(T resultObject, HttpStatusCode httpStatusCode);
    
    public class HttpClient
    {
        public string BearerToken { get; private set; }
        public bool IsAuthenticated { get; private set; }
        public string HostUrl { get; set; }

        public HttpClient() { }

        public HttpClient(string hostUrl)
        {
            HostUrl = hostUrl;
        }

        public bool Authenticate(string apiUrl, string username, string password)
        {
            var requestObject = new {Username = username, Password = password};
            Post<string>(ValidateUrl(apiUrl), requestObject, (token, statusCode) =>
            {
                if (statusCode != HttpStatusCode.OK || token == "") return;
                BearerToken = token;
                IsAuthenticated = true;
            });
            
            return IsAuthenticated;
        }

//----------------------------------------------------------------------------------------------------------------------

        public void Get<T>(string url, ResponseHandler<T> responseHandler) where T : class
        {
            var requestHandler = new RequestHandler<T>(ValidateUrl(url), null, responseHandler);

            var thread = new Thread(requestHandler.GetRequest);
            thread.Start();
        }
        

        public void Post<T>(string url, ResponseHandler<T> responseHandler) where T : class
        {
            Post(ValidateUrl(url), new object(), responseHandler);
        }
        
//----------------------------------------------------------------------------------------------------------------------
        public void Post<T>(string url, object requestObject, ResponseHandler<T> responseHandler) where T : class
        {
            var requestHandler = new RequestHandler<T>(ValidateUrl(url), requestObject, responseHandler) {BearerToken = BearerToken};

            var thread = new Thread(requestHandler.PostRequest);
            thread.Start();
        }

        private string ValidateUrl(string url)
        {
            return url.Contains(HostUrl) ? url : HostUrl + url;
        }
    }
}
