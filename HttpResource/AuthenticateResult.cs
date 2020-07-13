namespace HttpResource
{
    public class AuthenticateResult
    {
        public AuthenticateStatusCode AuthenticateStatusCode { get; set; }

        public string token { get; set; }
    }
}