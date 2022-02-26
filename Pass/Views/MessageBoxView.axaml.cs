using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace Pass.Views;

public class MessageBoxView : UserControl
{
    public MessageBoxView()
    {
        InitializeComponent();
    }

    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
    }
}