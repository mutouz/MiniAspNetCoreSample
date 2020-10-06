using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MiniAspNetCoreSample
{
    public interface IPipelineBuilder<TContext>
    {
        IPipelineBuilder<TContext> Use(Func<Action<TContext>, Action<TContext>> middleware);

        Action<TContext> Build();

        IPipelineBuilder<TContext> New();
    }

    public class PipelineBuilder
    {
        public static IPipelineBuilder<TContext> Create<TContext>(
            Action<TContext> completeAction)
        {
            return (IPipelineBuilder<TContext>)new PipelineBuilder<TContext>(completeAction);
        }

        public static IAsyncPipelineBuilder<TContext> CreateAsync<TContext>(
            Func<TContext, Task> completeFunc)
        {
            return (IAsyncPipelineBuilder<TContext>)new AsyncPipelineBuilder<TContext>(completeFunc);
        }
    }

    public class PipelineBuilder<TContext> : IPipelineBuilder<TContext>
    {
        private readonly Action<TContext> _completeFunc;

        private readonly IList<Func<Action<TContext>, Action<TContext>>> _pipelines =
            new List<Func<Action<TContext>, Action<TContext>>>();

        public PipelineBuilder(Action<TContext> completeFunc)
        {
            _completeFunc = completeFunc;
        }

        public IPipelineBuilder<TContext> Use(Func<Action<TContext>, Action<TContext>> middleware)
        {
            _pipelines.Add(middleware);
            return this;
        }

        public static PipelineBuilder<TContext> New(Action<TContext> completeFunc)
        {
            return new PipelineBuilder<TContext>(completeFunc);
        }

        public Action<TContext> Build()
        {
            var request = _completeFunc;
            foreach (var pipeline in _pipelines.Reverse())
            {
                request = pipeline(request);
            }

            return request;
        }

        public IPipelineBuilder<TContext> New()
        {
            return (IPipelineBuilder<TContext>)new PipelineBuilder<TContext>(this._completeFunc);
        }

        public static IAsyncPipelineBuilder<T> CreateAsync<T>(Func<T, Task> func)
        {
            return new AsyncPipelineBuilder<T>(func);
        }
    }
}