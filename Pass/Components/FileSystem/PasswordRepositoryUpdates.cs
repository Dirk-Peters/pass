using System.Threading.Tasks;
using bridgefield.FoundationalBits;
using Pass.Components.Encryption;
using Pass.Models;
using Pass.ViewModels;

namespace Pass.Components.FileSystem;

public sealed record NewPasswordCreated(Password Password);

public sealed class PasswordRepositoryUpdates
    : IHandleAsync<NewPasswordCreated>
{
    private readonly PasswordRepository passwords;
    private readonly KeyRepository keyRepository;
    private readonly IMessageBus messageBus;

    public PasswordRepositoryUpdates(
        PasswordRepository passwords,
        KeyRepository keyRepository,
        IMessageBus messageBus)
    {
        this.passwords = passwords;
        this.keyRepository = keyRepository;
        this.messageBus = messageBus;
    }

    public async Task Handle(NewPasswordCreated message)
    {
        await messageBus.Publish(new StartProgress());
        await passwords.EncryptPassword(keyRepository, message.Password);
        await messageBus.Publish(new EndProgress());
    }
}