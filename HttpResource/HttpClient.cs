using System.Net;
using System.Threading;

namespace HttpResource
{
    public delegate void ResponseHandler<T>(T resultObject, HttpStatusCode httpStatusCode);
    
    public class HttpClient
    {
        public string BearerToken { get; private set; }
        public bool IsAuthenticated { get; private set; }

        public HttpClient() { }

        public HttpClient(string loginApiUrl, string username, string password)
        {
            Authenticate(loginApiUrl, username, password);
        }

        public bool Authenticate(string apiUrl, string username, string password)
        {
            var requestObject = new {Username = username, Password = password};
            Post<string>(apiUrl, requestObject, (token, statusCode) =>
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
            var requestHandler = new RequestHandler<T>(url, null, responseHandler);

            var thread = new Thread(requestHandler.GetRequest);
            thread.Start();
        }
        

        public void Post<T>(string url, ResponseHandler<T> responseHandler) where T : class
        {
            Post(url, new object(), responseHandler);
        }
        
//----------------------------------------------------------------------------------------------------------------------
        public void Post<T>(string url, object requestObject, ResponseHandler<T> responseHandler) where T : class
        {
            var requestHandler = new RequestHandler<T>(url, requestObject, responseHandler) {BearerToken = BearerToken};

            var thread = new Thread(requestHandler.PostRequest);
            thread.Start();
        }
    }
}
