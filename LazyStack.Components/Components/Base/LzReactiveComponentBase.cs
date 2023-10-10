﻿// Copyright (c) 2022 .NET Foundation and Contributors. All rights reserved.
// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for full license information.

// LazyStack modified to include handling for OnInitializedAsync() missing 
// from ReactiveUI implementaiton.

using System.ComponentModel;
using Microsoft.AspNetCore.Components;
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
public class LzReactiveComponentBase<T> : ComponentBase, IViewFor<T>, INotifyPropertyChanged, ICanActivate, IDisposable
    where T : class, INotifyPropertyChanged
{
    private readonly Subject<Unit> _initSubject = new();
    [SuppressMessage("Design", "CA2213: Dispose object", Justification = "Used for deactivation.")]
    private readonly Subject<Unit> _deactivateSubject = new();
    private readonly CompositeDisposable _compositeDisposable = new();

    private T? _viewModel;

    private bool _disposedValue; // To detect redundant calls

    /// <inheritdoc />
    public event PropertyChangedEventHandler? PropertyChanged;

    /// <inheritdoc />
    [Parameter]
    public T? ViewModel
    {
        get => _viewModel;
        set
        {
            if (EqualityComparer<T?>.Default.Equals(_viewModel, value))
            {
                return;
            }

            _viewModel = value;
            OnPropertyChanged();
        }
    }

    [Inject]
    public T _myViewModel { set => ViewModel = value; }

    [Inject]
    public IMessages? Messages { get; set; }
    protected MarkupString Msg(string key) => (MarkupString)Messages!.Msg(key);

    /// <inheritdoc />
    object? IViewFor.ViewModel
    {
        get => ViewModel;
        set => ViewModel = (T?)value;
    }

    /// <inheritdoc />
    public IObservable<Unit> Activated => _initSubject.AsObservable();

    /// <inheritdoc />
    public IObservable<Unit> Deactivated => _deactivateSubject.AsObservable();

    /// <inheritdoc />
    public void Dispose()
    {
        // Do not change this code. Put cleanup code in Dispose(bool disposing) below.
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    /// <inheritdoc />
    protected override void OnInitialized()
    {
        _initSubject.OnNext(Unit.Default);
        base.OnInitialized();
    }

    protected override async Task OnInitializedAsync()
    {
        _initSubject.OnNext(Unit.Default);
        await base.OnInitializedAsync();
    }

    /// <inheritdoc/>
    protected override void OnAfterRender(bool firstRender)
    {
        if (firstRender)
        {
            // The following subscriptions are here because if they are done in OnInitialized, they conflict with certain JavaScript frameworks.
            var viewModelChanged =
                this.WhenAnyValue(x => x.ViewModel)
                    .Where(x => x is not null)
                    .Publish()
                    .RefCount(2);

            viewModelChanged
                .Subscribe(_ => InvokeAsync(StateHasChanged))
                .DisposeWith(_compositeDisposable);

            viewModelChanged
                .WhereNotNull()
                .Select(x =>
                            Observable
                                .FromEvent<PropertyChangedEventHandler?, Unit>(
                                                                               eventHandler =>
                                                                               {
                                                                                   void Handler(object? sender, PropertyChangedEventArgs e) => eventHandler(Unit.Default);
                                                                                   return Handler;
                                                                               },
                                                                               eh => x.PropertyChanged += eh,
                                                                               eh => x.PropertyChanged -= eh))
                .Switch()
                .Subscribe(_ => InvokeAsync(StateHasChanged))
                .DisposeWith(_compositeDisposable);
        }

        base.OnAfterRender(firstRender);
    }

    /// <summary>
    /// Invokes the property changed event.
    /// </summary>
    /// <param name="propertyName">The name of the property.</param>
    protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

    /// <summary>
    /// Cleans up the managed resources of the object.
    /// </summary>
    /// <param name="disposing">If it is getting called by the Dispose() method rather than a finalizer.</param>
    protected virtual void Dispose(bool disposing)
    {
        if (!_disposedValue)
        {
            if (disposing)
            {
                _initSubject.Dispose();
                _compositeDisposable.Dispose();
                _deactivateSubject.OnNext(Unit.Default);
            }

            _disposedValue = true;
        }
    }
}