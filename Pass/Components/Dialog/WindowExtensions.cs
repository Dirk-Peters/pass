using System;
using System.Threading.Tasks;
using Avalonia.Controls;

namespace Pass.Components.Dialog;

public static class WindowExtensions
{
    public static Task ShowDialog(this Window window, Window parent, Action<Window> onShowDialog)
    {
        Console.WriteLine($"Showing new dialog from thread {Environment.CurrentManagedThreadId}");
        onShowDialog(window);
        return window.ShowDialog(parent);
    }
}