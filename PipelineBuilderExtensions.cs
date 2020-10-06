using System;
using System.Threading.Tasks;

namespace MiniAspNetCoreSample
{
    public static class PipelineBuilderExtensions
    {
        public static IPipelineBuilder<TContext> Use<TContext>(
        this IPipelineBuilder<TContext> builder,
        Action<TContext, Action> action)
        {
            return builder.Use((Func<Action<TContext>, Action<TContext>>)(next => (Action<TContext>)(context => action(context, (Action)(() => next(context))))));
        }

        public static IPipelineBuilder<TContext> Use<TContext>(
          this IPipelineBuilder<TContext> builder,
          Action<TContext, Action<TContext>> action)
        {
            return builder.Use((Func<Action<TContext>, Action<TContext>>)(next => (Action<TContext>)(context => action(context, next))));
        }

        public static IPipelineBuilder<TContext> Run<TContext>(
          this IPipelineBuilder<TContext> builder,
          Action<TContext> handler)
        {
            return builder.Use((Func<Action<TContext>, Action<TContext>>)(_ => handler));
        }

        public static IPipelineBuilder<TContext> When<TContext>(
          this IPipelineBuilder<TContext> builder,
          Func<TContext, bool> predict,
          Action<IPipelineBuilder<TContext>> configureAction)
        {
            builder.Use<TContext>((Action<TContext, Action>)((context, next) =>
            {
                if (predict(context))
                {
                    IPipelineBuilder<TContext> pipelineBuilder = builder.New();
                    configureAction(pipelineBuilder);
                    pipelineBuilder.Build()(context);
                }
                else
                    next();
            }));
            return builder;
        }

        public static IAsyncPipelineBuilder<TContext> When<TContext>(
          this IAsyncPipelineBuilder<TContext> builder,
          Func<TContext, bool> predict,
          Action<IAsyncPipelineBuilder<TContext>> configureAction)
        {
            builder.Use<TContext>((Func<TContext, Func<Task>, Task>)((context, next) =>
            {
                if (!predict(context))
                    return next();
                IAsyncPipelineBuilder<TContext> asyncPipelineBuilder = builder.New();
                configureAction(asyncPipelineBuilder);
                return asyncPipelineBuilder.Build()(context);
            }));
            return builder;
        }

        public static IAsyncPipelineBuilder<TContext> Use<TContext>(
          this IAsyncPipelineBuilder<TContext> builder,
          Func<TContext, Func<Task>, Task> func)
        {
            return builder.Use((Func<Func<TContext, Task>, Func<TContext, Task>>)(next => (Func<TContext, Task>)(context => func(context, (Func<Task>)(() => next(context))))));
        }

        public static IAsyncPipelineBuilder<TContext> Use<TContext>(
          this IAsyncPipelineBuilder<TContext> builder,
          Func<TContext, Func<TContext, Task>, Task> func)
        {
            return builder.Use((Func<Func<TContext, Task>, Func<TContext, Task>>)(next => (Func<TContext, Task>)(context => func(context, next))));
        }

        public static IAsyncPipelineBuilder<TContext> Run<TContext>(
          this IAsyncPipelineBuilder<TContext> builder,
          Func<TContext, Task> handler)
        {
            return builder.Use((Func<Func<TContext, Task>, Func<TContext, Task>>)(_ => handler));
        }
    }
}