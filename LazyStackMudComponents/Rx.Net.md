# Rx.Net 
References:
https://github.com/dotnet/reactive
http://introtorx.com/
https://reactivex.io/
https://www.reactiveui.net/

ReactiveUI.Blazor 

Notes on ReactiveUI in Blazor

## What it is:

### Quick recap on INotifyPropertyChanged in MVVM
- ViewModels implement INotifyPropertyChanged
- When properties are set, a C# event is raised
- Views register callbacks (hooks into the C# events) and can take action to update their state
- This approach works but introduces a tight coupling among ViewModel and View.
- This is a very chatty connection as every change in the ViewModel generates an event which is handled by the View

### Overview of ReactiveUI
- Not primarily a Mediator Pattern; there is no central mediator instance unless you use MessageBus
- Is a Pub/Sub pattern for composing asynchronous and event-based programs using observable sequences and LINQ-style query operators. Using Rx, developers represent asynchronous data streams with Observables, query asynchronous data streams using LINQ operators, and parameterize the concurrency in the asynchronous data streams using Schedulers
    - Observables - a data stream of events
    - Query Observables with LINQ
    - Manage concurrency in Observables using Schedulers
    - Adds a "time/sequence" dimension to events
- Direct support for MVVM with substantial new optimizations

### ViewModel ReactiveObject
- ReactiveObject is the base object for ViewModel classes.
    - It implements INotifyPropertyChanged and INotifyPropertyChanging (the C# event model) to add to an IObservable property
    - The events are of type IReactivePropertyChangedEvent
        - PropertyName
        - Sender
    - Events in the ViewModel add to the IObservable properties:
        - Changing
        - Changed
        - ThrownExceptions
    - It provides some settings and state methods:
        - SuppressChangeNotifications()
        - AreChangeNotificationsEnabled()
        - DelayChangeNotifications()

A ViewModel : ReactiveObject will typically have one or more data properties with ObservablePropertyHelper<T> backing fields so that changes to the data properties result in IObservable events.

ViewModel constructors generally assign ICommands using ReactiveCommand methods like:
    - synchronous
        - Create()
        - CreateCombined() - exeucte one or more child commands
    - asynchronous 
        - CreateFromObservable() 
        - CreateFromTask()
    - Create

#### ReactiveCommand : ICommand, IObservable, ...
Encapsulates a user action behind a reactive interface. 
    - CanExecute
    - IsExecuting 
    - ThrownExceptions
    - Subscribe()
    - Exceute()

### View : ReactiveComponentBase<T>
    - T ViewModel holds reference to ViewModel : ReactiveObject
    - OnAfterRender
        - 


## Getting Started

ReactiveUI is a large and complex tool. It will take some time to learn and use to best advantage. To get going, we will use a subset of common patterns that provide most of what we need (80/20 rule).

- Use ViewModels that are instrumented with ReactiveUI - use CQRS (Command Query Responsibility Segregation) pattern
    - Querys {class}ViewModelQuery - RL operations
    - Commands {class}ViewModel - CUD operations
- Use Views (Components) that follow the pattern provided by our samples
- Use ReactiveUi.Commands 
- Use the Models and Repo SDK generaged by LazyStackMDD
    - We can generate classes that implemnet INotifyPropertyChanged if that is deemed useful


## Modeling Architecture

The cust.Svc library contains calls to REST Interface.
The cust.ViewModel librarys contains IViewModel<T> interface defintiions and ViewModel<T>
The cust.Components library contains components that Inherit IViewModel<T>.
The cust.App library contains a ViewModels folder that contains imiplementations of the IViewModel<> interface.





