using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;

namespace MiniAspNetCoreSample
{
    public class HttpListenerServer : IServer
    {
        private readonly HttpListener _httpListener;
        private readonly IServiceProvider _serviceProvider;

        public HttpListenerServer(IServiceProvider serviceProvider, IConfiguration configuration)
        {
            _httpListener = new HttpListener();
            var urls = configuration.GetAppSetting("ASPNETCORE_URLS")?.Split(';');
            if (urls != null && urls.Length > 0)
            {
                foreach (var url in urls
                    .Where(string.IsNullOrEmpty)
                    .Select(u => u.Trim())
                    .Distinct()
                )
                {
                    // Prefixes must end in a forward slash ("/")
                    // https://stackoverflow.com/questions/26157475/use-of-httplistener
                    _httpListener.Prefixes.Add(url.EndsWith("/") ? url : $"{url}/");
                }
            }
            else
            {
                _httpListener.Prefixes.Add("http://localhost:5100/");
            }

            _serviceProvider = serviceProvider;
        }

        public async Task StartAsync(Func<HttpContext, Task> requestHandler, CancellationToken cts = default)
        {
            _httpListener.Start();
            if (_httpListener.IsListening)
            {
                Console.WriteLine("the server is listening on ");
                Console.WriteLine(string.Join(",", _httpListener.Prefixes));
            }

            while (!cts.IsCancellationRequested)
            {
                var listenerContext = await _httpListener.GetContextAsync();

                var featureCollection = new FeatureCollection();

                featureCollection.Set(listenerContext.GetRequestFeature());
                featureCollection.Set(listenerContext.GetResponseFeature());

                using (var scope = _serviceProvider.CreateScope())
                {
                    var httpContext = new HttpContext(featureCollection)
                    {
                        RequestServices = scope.ServiceProvider,
                    };
                    await requestHandler(httpContext);
                    listenerContext.Response.Close();
                }
            }
            _httpListener.Stop();
        }
    }
}