using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace Pass.Views;

public sealed class NewPasswordView : UserControl
{
    public NewPasswordView() => InitializeComponent();

    private void InitializeComponent() => AvaloniaXamlLoader.Load(this);
}