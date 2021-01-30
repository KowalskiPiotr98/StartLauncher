using System.Net;
using System.Net.Http;

namespace StartLauncher.Utilities.Updater
{
    class GitHubClient : HttpClient
    {
        public GitHubClient() : base()
        {
            DefaultRequestHeaders.UserAgent.Add(new System.Net.Http.Headers.ProductInfoHeaderValue("StartLauncher", App.Version));
        }
    }

    class GitHubWebClient : WebClient
    {
        public GitHubWebClient() : base()
        {
            Headers.Add(HttpRequestHeader.UserAgent, $"StartLauncher/{App.Version}");
        }
    }
}
