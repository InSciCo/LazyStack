using System.ComponentModel;
using Microsoft.AspNetCore.Components;
using ReactiveUI.Blazor;
using LazyStack.Utils;

namespace LazyStack.Components;

public class CoreComponentBasePassViewModel<T> : ReactiveComponentBase<T>
    where T : class, INotifyPropertyChanged
{

    [Inject]
    public IMessages? Messages { get; set;  }

    protected MarkupString Msg(string key) => (MarkupString)Messages!.Msg(key);

}
