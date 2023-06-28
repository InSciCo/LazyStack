
using System.Reactive.Linq;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using LazyStackAuth;

namespace LazyStack.ViewModels;

public class AuthViewModelBase<T> : ReactiveObject
    where T : class
{
    public AuthViewModelBase(
        IAuthProcess authProcess
        )
    {
        this.AuthProcess = authProcess;
        AuthProcess
            .WhenAnyValue(x => x.IsSignedIn)
            .Subscribe(x => IsActive = x);
    }

    [Reactive]
    public IAuthProcess? AuthProcess { get; set; }
    [Reactive]
    public bool IsActive { get; set; }
    [Reactive]
    public T? Data { get; set; }

}
