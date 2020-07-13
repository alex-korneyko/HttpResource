using System.Net;
using System.Threading;

namespace HttpResource
{
    public delegate void ResponseHandler<T>(T resultObject, HttpStatusCode httpStatusCode);

    public delegate void AuthenticateHandler(AuthenticateResult result);
    
    public static class HttpClient
    {
        public static string BearerToken { get; private set; }
        public static bool IsAuthenticated { get; private set; }
        public static string HostUrl { get; set; }

        public static void Authenticate(string apiUrl, string username, string password)
        {
            Authenticate(apiUrl, username, password, a => {});
        }
        
        public static void Authenticate(string apiUrl, string username, string password, AuthenticateHandler authenticateHandler)
        {
            var requestObject = new {Username = username, Password = password};
            PostAsync<AuthenticateResult>(ValidateUrl(apiUrl), requestObject, (authenticateResult, statusCode) =>
            {
                if (statusCode != HttpStatusCode.OK) return;

                if (authenticateResult.AuthenticateStatusCode == AuthenticateStatusCode.OK)
                {
                    BearerToken = authenticateResult.token;
                    IsAuthenticated = true;
                }
                
                authenticateHandler.Invoke(authenticateResult);
            });
        }

//----------------------------------------------------------------------------------------------------------------------

        public static void Get<T>(string url, ResponseHandler<T> responseHandler) where T : class
        {
            var requestHandler = new RequestHandler<T>(ValidateUrl(url), null, responseHandler);

            var thread = new Thread(requestHandler.GetRequest);
            thread.Start();
        }

        public static void Post<T>(string url, ResponseHandler<T> responseHandler) where T : class
        {
            Post(url, new object(), responseHandler);
        }

        public static void Post<T>(string url, object requestObject, ResponseHandler<T> responseHandler) where T : class
        {
            var requestHandler = new RequestHandler<T>(ValidateUrl(url), requestObject, responseHandler) {BearerToken = BearerToken};
            requestHandler.PostRequest();
        }

        public static void PostAsync<T>(string url, ResponseHandler<T> responseHandler) where T : class
        {
            PostAsync(ValidateUrl(url), new object(), responseHandler);
        }
        
//----------------------------------------------------------------------------------------------------------------------
        public static void PostAsync<T>(string url, object requestObject, ResponseHandler<T> responseHandler) where T : class
        {
            var requestHandler = new RequestHandler<T>(ValidateUrl(url), requestObject, responseHandler) {BearerToken = BearerToken};

            var thread = new Thread(requestHandler.PostRequest);
            thread.Start();
        }

        private static string ValidateUrl(string url)
        {
            return url.Contains(HostUrl) ? url : HostUrl + url;
        }
    }
}
