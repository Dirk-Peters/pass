using System.Threading.Tasks;
using bridgefield.FoundationalBits;

namespace Pass.Components.Navigation;

public static class MessageBusExtensions
{
    public static Task Publish<TMessage>(this IMessageBus messageBus) where TMessage : new() =>
        messageBus.Publish(new TMessage());
}