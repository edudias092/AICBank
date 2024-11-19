namespace AICBank.Core.Util;

public class HttpRequestBuilder
{
    private HttpRequestMessage Request { get; init; }
    private HttpRequestBuilder(string uri, HttpMethod method)
    {
        Request = new HttpRequestMessage(method, uri);
    }
    
    private HttpRequestBuilder(Uri uri, HttpMethod method)
    {
        Request = new HttpRequestMessage(method, uri);
    }
    private HttpRequestBuilder() { }
    
    public static HttpRequestBuilder Create(string uri, HttpMethod method)
    {
        return new HttpRequestBuilder(uri, method);
    }
    
    public static HttpRequestBuilder Create(Uri uri, HttpMethod method)
    {
        return new HttpRequestBuilder(uri, method);
    }

    public HttpRequestBuilder AddHeader(string key, string value)
    {
        Request.Headers.Add(key, value);

        return this;
    }

    public HttpRequestBuilder AddAuthorization(string scheme, string parameter)
    {
        Request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue(scheme, parameter);
        
        return this;
    }

    public HttpRequestBuilder AddContent(HttpContent content)
    {
        Request.Content = content;

        return this;
    }

    public HttpRequestMessage Build()
    {
        return Request;
    }
}