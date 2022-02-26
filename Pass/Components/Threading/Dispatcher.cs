using System;
using System.Threading.Tasks;
using MonadicBits;
using Pass.Components.Extensions;

namespace Pass.Components.Threading;

public static class Dispatcher
{
    public static void Dispatch(Action action)
    {
        Console.WriteLine($"Dispatching from thread {Environment.CurrentManagedThreadId}");
        Avalonia.Threading.Dispatcher.UIThread
            .JustWhen(d => !d.CheckAccess())
            .Match(d => d.InvokeAsync(action), action);
    }

    private static Task Dispatch(
        this Avalonia.Threading.Dispatcher dispatcher,
        Func<Task> func)
    {
        Console.WriteLine($"Dispatching async from thread {Environment.CurrentManagedThreadId}");
        return dispatcher
            .JustWhen(d => !d.CheckAccess())
            .Match(d =>
            {
                Console.WriteLine("dispatching func to UI Thread");
                return d.InvokeAsync(func);
            }, func);
    }

    public static Task Dispatch(Func<Task> func)
    {
        Console.WriteLine($"Dispatching with task completion source from thread {Environment.CurrentManagedThreadId}");
        var taskCompletionSource = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);

        Avalonia.Threading.Dispatcher.UIThread
            .Dispatch(func)
            .ContinueWith(
                task => Task.Run(() => task.IsFaulted
                    .OnTrue(() =>
                    {
                        Console.WriteLine(task.Exception);
                        taskCompletionSource.SetException(task.Exception!);
                    })
                    .OnFalse(() =>
                    {
                        Console.WriteLine("dispatch finished");
                        taskCompletionSource.SetResult();
                    })));

        // Dispatch((Action) (() => func().ContinueWith(
        //     task => task.IsFaulted
        //         .OnTrue(() => taskCompletionSource.SetException(task.Exception!))
        //         .OnFalse(() => taskCompletionSource.SetResult()))));

        return taskCompletionSource.Task;
    }

    // public static Task<T> Dispatch<T>(Func<Task<T>> func)
    // {
    //     var taskCompletionSource = new TaskCompletionSource<T>(TaskCreationOptions.RunContinuationsAsynchronously);
    //
    //     Dispatch((Action) (() => func().ContinueWith(
    //         task => task.IsFaulted
    //             .OnTrue(() => taskCompletionSource.SetException(task.Exception!))
    //             .OnFalse(() => taskCompletionSource.SetResult(task.Result)))));
    //
    //     return taskCompletionSource.Task;
    // }
}