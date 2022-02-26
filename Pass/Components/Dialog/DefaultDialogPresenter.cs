using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Avalonia.Controls;
using bridgefield.FoundationalBits;
using Pass.Components.Threading;
using Pass.ViewModels;
using Pass.Views;

namespace Pass.Components.Dialog;

public sealed class DefaultDialogPresenter : IDialogPresenter, IHandleAsync<OkMessage>
{
    private readonly Stack<Window> stack = new();
    private Window Current => stack.Peek();

    public DefaultDialogPresenter(Window window) => stack.Push(window);

    public Task Show(IDialog dialog) =>
        Dispatcher.Dispatch(() =>
        {
            Console.WriteLine($"Opening dialog from thread {Environment.CurrentManagedThreadId}");
            return DialogView(dialog).ShowDialog(Current, stack.Push);
        });

    private DialogView DialogView(IDialog dialog)
    {
        var view = new DialogView { DataContext = dialog };
        dialog.Closed += (_, _) => Dispatcher.Dispatch(view.Close);
        view.Closing += (_, _) => stack.Pop();
        return view;
    }

    public async Task Handle(OkMessage message) =>
        await Show(new OkMessageBoxViewModel
        {
            Title = message.Title,
            Message = message.Message,
            MinHeight = 40,
            MinWidth = 80
        });
}