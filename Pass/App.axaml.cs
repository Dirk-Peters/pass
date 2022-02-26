using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using bridgefield.FoundationalBits;
using Bridgefield.PersistentBits;
using Bridgefield.PersistentBits.FileSystem;
using Microsoft.Extensions.Configuration;
using MonadicBits;
using Pass.Components.Dialog;
using Pass.Components.Encryption;
using Pass.Components.Extensions;
using Pass.ViewModels;
using Pass.Views;

namespace Pass;

using static Functional;

public sealed class App : Application, IHandle<object>
{
    public override void Initialize() => AvaloniaXamlLoader.Load(this);

    public override void OnFrameworkInitializationCompleted()
    {
        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            var configuration = Configuration(Environment.GetCommandLineArgs());
            var fileSystem = OS.FileSystem();
            var passwordDirectory = fileSystem
                .OpenDirectory(configuration.Filesystem.PasswordStorePath.Path)
                .Match(d => d, () => throw new ArgumentException("Pass directory is missing!"));
            var keyRepository = KeyRepository(fileSystem, configuration);
            var messageBus = MessageBus.Create();

            var mainWindow = new MainView();
            var dialogPresenter = new DefaultDialogPresenter(mainWindow);
            messageBus.Subscribe(dialogPresenter, SubscriptionLifecycle.ExplicitUnsubscribe);
            messageBus.Subscribe(this);
            desktop.MainWindow = mainWindow;
            mainWindow.DataContext = new MainViewModel(passwordDirectory, keyRepository, messageBus);
            Handle("Application ready");
        }

        base.OnFrameworkInitializationCompleted();
    }

    private static KeyRepository KeyRepository(IFileSystem fileSystem, AppSettings appSettings)
    {
        var directory = fileSystem.OpenDirectory(appSettings.Filesystem.KeyStoragePath.Path)
            .Match(d => d, () => throw new ArgumentException("Key directory is missing!"));

        return new KeyRepository
        {
            PrivateKey = directory.Files.Where(file => file.Name == "private.asc").SingleOrNothing(),
            PublicKey = directory.Files.Where(file => file.Name == "public.asc").SingleOrNothing(),
            Password = Nothing
        };
    }

    private static AppSettings Configuration(string[] args)
    {
        var settings = new AppSettings();
        new ConfigurationBuilder()
            .AddJsonFile("appsettings.json")
            .AddEnvironmentVariables()
            .AddCommandLine(args)
            .Build()
            .Bind(settings, o => o.BindNonPublicProperties = true);
        return settings;
    }

    public void Handle(object message) =>
        Console.WriteLine($"Message dispatched: {message} in thread {Environment.CurrentManagedThreadId}");
}