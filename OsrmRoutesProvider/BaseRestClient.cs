using System;
using RestSharp;

namespace OsrmRoutesProvider
{
    /// <summary>
    /// REST client for Map Api Provider
    /// </summary>
    public abstract class BaseRestClient
    {
        public IRestClient Client { get; set; }

        protected BaseRestClient()
        {
            Client = new RestClient();
        }

        protected BaseRestClient(string baseUrl) : this()
        {
            Client.BaseUrl = new Uri(baseUrl);
        }
    }
}
