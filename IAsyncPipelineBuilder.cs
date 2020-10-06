using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MiniAspNetCoreSample
{
    public interface IAsyncPipelineBuilder<TContext>
    {
        IAsyncPipelineBuilder<TContext> Use(Func<Func<TContext, Task>, Func<TContext, Task>> middleware);

        Func<TContext, Task> Build();

        IAsyncPipelineBuilder<TContext> New();
    }

    public class AsyncPipelineBuilder<TContext> : IAsyncPipelineBuilder<TContext>
    {
        private readonly Func<TContext, Task> _completeFunc;

        private readonly IList<Func<Func<TContext, Task>, Func<TContext, Task>>> _pipelines
            = new List<Func<Func<TContext, Task>, Func<TContext, Task>>>();

        public AsyncPipelineBuilder(Func<TContext, Task> completeFunc)
        {
            _completeFunc = completeFunc;
        }

        public IAsyncPipelineBuilder<TContext> Use(Func<Func<TContext, Task>, Func<TContext, Task>> middleware)
        {
            _pipelines.Add(middleware);
            return this;
        }

        public IAsyncPipelineBuilder<TContext> New()
        {
            return (IAsyncPipelineBuilder<TContext>)new AsyncPipelineBuilder<TContext>(this._completeFunc);
        }

        public Func<TContext, Task> Build()
        {
            // 数组反转用来反转 pipeline
            return _pipelines.Reverse().Aggregate(_completeFunc, (current, pipeline) => pipeline(current));
        }
    }
}