﻿using System;
using System.Collections.Generic;
using Pass.Components.Binding;
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