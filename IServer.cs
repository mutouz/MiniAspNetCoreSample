
using System;
using System.Threading;
using System.Threading.Tasks;

namespace MiniAspNetCoreSample
{
    public interface IServer
    {
        Task StartAsync(Func<HttpContext, Task> requestHandler, CancellationToken cts = default);
    }
}