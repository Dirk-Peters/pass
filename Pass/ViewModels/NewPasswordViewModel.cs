using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Reactive.Linq;
using System.Windows.Input;
using bridgefield.FoundationalBits;
using Pass.Components.Binding;
using Pass.Components.Commands;
using Pass.Components.FileSystem;
using Pass.Components.Navigation;
using Pass.Components.ViewMapping;
using Pass.Models;
using Pass.Views;

namespace Pass.ViewModels;

[View(typeof(NewPasswordView))]
public sealed class NewPasswordViewModel : Bindable, INotifyDataErrorInfo
{
    private readonly IMessageBus messageBus;
    private readonly ReactiveProperty<string> nameProperty = new();
    private readonly ReactiveProperty<string> passwordProperty = new();
    private readonly ReactiveProperty<string> passwordConfirmationProperty = new();
    private readonly ReactiveProperty<string> passwordValidationError = new();

    public string Name
    {
        get => nameProperty.Value;
        set => nameProperty.Value = value;
    }

    public string Password
    {
        get => passwordProperty.Value;
        set => passwordProperty.Value = value;
    }

    public string PasswordConfirmation
    {
        get => passwordConfirmationProperty.Value;
        set => passwordConfirmationProperty.Value = value;
    }

    public NewPasswordViewModel(IMessageBus messageBus)
    {
        this.messageBus = messageBus;
        nameProperty
            .Changed
            .Select(_ => nameof(Name))
            .Merge(passwordProperty.Changed.Select(_ => nameof(Password)))
            .Merge(passwordConfirmationProperty.Changed.Select(_ => nameof(PasswordConfirmation)))
            .Subscribe(OnPropertyChanged);

        passwordProperty
            .CombineLatest(passwordConfirmationProperty, nameProperty)
            .Select(tuple => tuple switch
            {
                _ when string.IsNullOrWhiteSpace(tuple.First)
                       && !string.IsNullOrWhiteSpace(tuple.Third) =>
                    "please select a password",
                _ when string.IsNullOrWhiteSpace(tuple.Second)
                       && !string.IsNullOrWhiteSpace(tuple.Third) =>
                    "please confirm your password",
                _ when tuple.First != tuple.Second =>
                    "confirmation does not match password",
                _ => string.Empty
            })
            .Subscribe(error => passwordValidationError.Value = error);
        passwordValidationError.Subscribe(_ =>
            ErrorsChanged?.Invoke(this, new DataErrorsChangedEventArgs(nameof(PasswordConfirmation))));
    }

    public ICommand Cancel => new RelayCommand(() => messageBus.Publish(new PopContent()));
    public ICommand Create => new CreatePasswordCommand(this);


    public IEnumerable GetErrors(string propertyName) =>
        propertyName switch
        {
            nameof(PasswordConfirmation) => new[] { passwordValidationError.Value },
            _ => string.Empty
        };

    public bool HasErrors => !string.IsNullOrEmpty(passwordValidationError.Value);
    public event EventHandler<DataErrorsChangedEventArgs> ErrorsChanged;

    private sealed class CreatePasswordCommand : CommandBase
    {
        private readonly NewPasswordViewModel viewModel;

        public CreatePasswordCommand(NewPasswordViewModel viewModel)
            : base(viewModel.passwordConfirmationProperty.Changed) =>
            this.viewModel = viewModel;

        protected override async void OnExecute(object parameter)
        {
            await viewModel.messageBus.Publish(
                new NewPasswordCreated(
                    new Password(viewModel.Name, viewModel.Password,
                        new Dictionary<string, string>())));
            await viewModel.messageBus.Publish<PopContent>();
        }

        protected override bool OnCanExecute(object parameter) =>
            !viewModel.HasErrors;
    }
}