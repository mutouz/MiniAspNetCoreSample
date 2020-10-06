using System;
using System.Threading;
using System.Threading.Tasks;

namespace MiniAspNetCoreSample
{
    internal class Program
    {
        private static readonly CancellationTokenSource Cts = new CancellationTokenSource();

        public static async Task Main(string[] args)
        {
            Console.CancelKeyPress += OnExit;

            var host = WebHost.WebHostBuilder.CreateDefault(args)
                .ConfigureServices((config, service) =>
                {
                })
                .ConfigureApplication((config, app) =>
                {
                    app.When(context => context.Request.Url.PathAndQuery.StartsWith("/favicon.ico"), pipeline => { });
                    app.When(context => context.Request.Url.PathAndQuery.Contains("test"),
                        p => p.Run(context => context.Response.WriteAsync("test")));

                    app
                        .Use(async (context, next) =>
                        {
                            Console.WriteLine("middleware1 start");
                            await context.Response.WriteLineAsync(
                                $"middleware1, requestPath:{context.Request.Url.AbsolutePath}");
                            await next();
                            Console.WriteLine("middleware1 end");

                        })
                        .Use(async (context, next) =>
                        {
                            Console.WriteLine("middleware2 start");
                            await context.Response.WriteLineAsync(
                                $"middleware2, requestPath:{context.Request.Url.AbsolutePath}");
                            await next();
                            Console.WriteLine("middleware2 end");
                        })
                        .Use(async (context, next) =>
                        {
                            Console.WriteLine("middleware3 start");
                            await context.Response.WriteLineAsync(
                                $"middleware3, requestPath:{context.Request.Url.AbsolutePath}");
                            await next();
                            Console.WriteLine("middleware3 end");
                        });

                    app.Run(context => context.Response.WriteAsync("Hello Mini Asp.Net Core"));
                })
                .Initialize((config, services) =>
                {
                })
                .Build();

            await host.RunAsync(Cts.Token);
        }

        private static void OnExit(object sender, ConsoleCancelEventArgs e)
        {
            Console.WriteLine("exiting ...");
            Cts.Cancel();
        }
    }
}