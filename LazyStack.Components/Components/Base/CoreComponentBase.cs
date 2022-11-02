using System.ComponentModel;
using Microsoft.AspNetCore.Components;
using ReactiveUI.Blazor;
using ReactiveUI;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Reactive;
using System.Diagnostics.CodeAnalysis;
using System.Reactive.Subjects;
using System.Runtime.CompilerServices;
using LazyStack.Utils;

namespace LazyStack.Components;

/// <summary>
/// A base component for handling property changes and updating the blazer view appropriately.
/// </summary>
/// <typeparam name="T">The type of view model. Must support INotifyPropertyChanged.</typeparam>
public class CoreComponentBase<T> : LzReactiveComponentBase<T>
    where T : class, INotifyPropertyChanged
{

    [Inject]
    new public T _myViewModel { set => ViewModel = value; }

    [Inject]
    new public IMessages? Messages { get; set; }
    new protected MarkupString Msg(string key) => (MarkupString)Messages!.Msg(key);

    /// <inheritdoc />

}