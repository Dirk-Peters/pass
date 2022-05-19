using System.Linq;
using System.Threading.Tasks;
using bridgefield.FoundationalBits;
using Pass.Components.Dialog;
using Pass.Components.Encryption;
using Pass.Components.Extensions;
using Pass.Components.Navigation;
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
        //await messageBus.Publish<StartProgress>();
        await (await passwords.All()
                .Any(pw => pw.Name == message.Password.Name)
                .OnTrue(async () => await messageBus.Publish(
                    new OkMessage("Duplicate Name", "A password with the given name already exists."))))
            .OnFalse(() => passwords.EncryptPassword(keyRepository, message.Password));
        //await messageBus.Publish<EndProgress>();
    }
}