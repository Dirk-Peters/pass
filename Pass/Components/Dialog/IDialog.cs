using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows.Input;
using Pass.Components.Binding;

namespace Pass.Components.Dialog;

public sealed record ActionItem(string DisplayText, ICommand Command) : BindableRecord;

public interface IDialog : INotifyPropertyChanged
{
    string Title { get; }
    public double MinWidth { get; }
    public double MinHeight { get; }
    public Bindable Content { get; }
    public IEnumerable<ActionItem> Buttons { get; }
    event EventHandler Closed;
    void Close();
}