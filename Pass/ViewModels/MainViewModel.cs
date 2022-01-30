﻿using System;
using System.Collections.Generic;
using bridgefield.FoundationalBits;
using Bridgefield.PersistentBits.FileSystem;
using MonadicBits;
using Pass.Components.Binding;
using Pass.Components.Encryption;
using Pass.Components.FileSystem;
using Pass.Components.Navigation;
using Pass.Components.ViewMapping;
using Pass.Views;

namespace Pass.ViewModels;

using static Functional;

[View(typeof(MainView))]
public sealed class MainViewModel
    : Bindable,
        IDisposable,
        IHandle<Unlocked>,
        IHandle<Locked>,
        IHandle<PushContent>,
        IHandle<PopContent>
{
    private readonly IDirectory passwordDirectory;
    private readonly KeyRepository keyRepository;
    private readonly IMessageBus messageBus;
    private readonly Stack<Bindable> contentStack = new();

    public Bindable Content => contentStack.Peek();

    public MainViewModel(
        IDirectory passwordDirectory,
        KeyRepository keyRepository,
        IMessageBus messageBus)
    {
        this.passwordDirectory = passwordDirectory;
        this.keyRepository = keyRepository;
        this.messageBus = messageBus;

        contentStack.Push(new UnlockViewModel(messageBus));

        messageBus.Subscribe(this);
    }

    public void Handle(Unlocked message)
    {
        keyRepository.Password = message.Password;

        Handle(new PushContent(new ContentWithSidebarViewModel(
            new TextViewModel("No password selected!"),
            new PasswordListViewModel(new PasswordRepository(passwordDirectory), messageBus, keyRepository),
            messageBus)));
    }

    public void Handle(Locked message)
    {
        keyRepository.Password = Nothing;
        Handle(new PopContent());
    }

    public void Dispose() => messageBus.Unsubscribe(this);

    public void Handle(PushContent message)
    {
        contentStack.Push(message.Content);
        OnPropertyChanged(nameof(Content));
    }

    public void Handle(PopContent message)
    {
        var popped = contentStack.Pop();
        if (popped is IDisposable disposable)
        {
            disposable.Dispose();
        }

        OnPropertyChanged(nameof(Content));
    }
}