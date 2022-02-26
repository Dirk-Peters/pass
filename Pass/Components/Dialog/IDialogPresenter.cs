using System.Threading.Tasks;

namespace Pass.Components.Dialog;

public sealed record OkMessage(string Title, string Message);

public interface IDialogPresenter
{
    Task Show(IDialog dialog);
}