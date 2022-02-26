using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Pass.Components.Binding;
using Pass.Components.Commands;
using Pass.Components.Dialog;
using Pass.Components.ViewMapping;
using Pass.Views;

namespace Pass.ViewModels;

[View(typeof(DialogView))]
public sealed class DefaultDialogViewModel : Bindable, IDialog
{
    public string Title => "Default Dialog";
    public double MinWidth => 500;
    public double MinHeight => 300;
    public Bindable Content { get; }
    public IEnumerable<ActionItem> Buttons { get; }

    public event EventHandler Closed;

    public DefaultDialogViewModel(
        Bindable content,
        Func<IDialog, IEnumerable<ActionItem>> buttons) =>
        (Content, Buttons) = (content, buttons.Invoke(this));

    public void Close() =>
        Closed?.Invoke(this, EventArgs.Empty);
}

public interface IMessageBoxViewModel
{
    public string Message { get; }
}

public sealed class OkMessageBoxViewModel : Bindable, IDialog, IMessageBoxViewModel
{
    public string Title { get; init; }
    public string Message { get; init; }
    public double MinWidth { get; init; }
    public double MinHeight { get; init; }
    public Bindable Content => this;

    public IEnumerable<ActionItem> Buttons => new[]
    {
        new ActionItem("Ok", new RelayCommand(
            () =>
            {
                Console.WriteLine("closing ok message box");
                Task.Run(Close);
                return Task.CompletedTask;
            }))
    };

    public event EventHandler Closed;

    public void Close() => Closed?.Invoke(this, EventArgs.Empty);
}