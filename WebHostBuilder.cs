using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace MiniAspNetCoreSample
{
    public interface IHost
    {
        Task RunAsync(CancellationToken cts = default);
    }

    public class WebHost : IHost
    {
        private readonly Func<HttpContext, Task> _requestDelegate;
        private readonly IServer _server;

        public WebHost(Func<HttpContext, Task> requestDelegate, IServiceProvider serviceProvider)
        {
            _requestDelegate = requestDelegate;
            _server = serviceProvider.GetRequiredService<IServer>();
        }

        public async Task RunAsync(CancellationToken cts = default)
        {
            await _server.StartAsync(_requestDelegate, cts).ConfigureAwait(false);
        }

        public interface IHostBuilder
        {
            IHostBuilder ConfigureConfiguration(Action<IConfigurationBuilder> configAction);

            IHostBuilder ConfigureServices(Action<IConfiguration, IServiceCollection> configAction);

            IHostBuilder Initialize(Action<IConfiguration, IServiceProvider> configAction);

            IHostBuilder ConfigureApplication(Action<IConfiguration, IAsyncPipelineBuilder<HttpContext>> configAction);

            IHost Build();
        }

        public class WebHostBuilder : IHostBuilder
        {
            private readonly IConfigurationBuilder _configurationBuilder = new ConfigurationBuilder();
            private readonly IServiceCollection _serviceCollection = new ServiceCollection();

            private Action<IConfiguration, IServiceProvider> _initAction;

            private readonly IAsyncPipelineBuilder<HttpContext> _reAsyncPipeline =
                PipelineBuilder.CreateAsync<HttpContext>(
                    context =>
                    {
                        context.Response.StatusCode = 404;
                        return Task.CompletedTask;
                    });

            public IHostBuilder ConfigureConfiguration(Action<IConfigurationBuilder> configAction)
            {
                configAction?.Invoke(_configurationBuilder);
                return this;
            }

            public IHostBuilder ConfigureServices(Action<IConfiguration, IServiceCollection> configAction)
            {
                if (null == configAction) return this;
                var configuration = _configurationBuilder.Build();
                configAction.Invoke(configuration, _serviceCollection);

                return this;
            }

            public IHostBuilder Initialize(Action<IConfiguration, IServiceProvider> configAction)
            {
                if (null != configAction)
                {
                    _initAction = configAction;
                }

                return this;
            }

            public IHostBuilder ConfigureApplication(Action<IConfiguration, IAsyncPipelineBuilder<HttpContext>> configAction)
            {
                if (null == configAction) return this;
                var configuration = _configurationBuilder.Build();
                configAction.Invoke(configuration, _reAsyncPipeline);
                return this;
            }

            public IHost Build()
            {
                var configuration = _configurationBuilder.Build();
                _serviceCollection.AddSingleton<IConfiguration>(configuration);
                var serviceProvider = _serviceCollection.BuildServiceProvider();

                _initAction?.Invoke(configuration, serviceProvider);

                return new WebHost(_reAsyncPipeline.Build(), serviceProvider);
            }

            public static WebHostBuilder CreateDefault(string[] args)
            {
                var webHostBuilder = new WebHostBuilder();
                webHostBuilder.ConfigureConfiguration(builder => builder.AddJsonFile("appsettings.json", true, true))
                    .UseHttpListenerServer();
                return webHostBuilder;
            }
        }
    }

    public static class WebHostBuilderExtensions
    {
        public static WebHost.IHostBuilder UseHttpListenerServer(this WebHost.IHostBuilder builder)
        {
            return builder.ConfigureServices((config, service) =>
            {
                service.AddSingleton<IServer, HttpListenerServer>();
            });
        }
    }
}