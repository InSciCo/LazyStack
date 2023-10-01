# ViewModel Guidelines

## General 
- All ViewModels should inherit from ReactiveObject. This implements INotifyPropertyChanged.
- All ViewModels shouuld use ReactiveUI.Fody.Helpers where possible. [Reactive] [ObservableAsProperty]
- Always use a ViewModel, never directly hook up a View directly to the ClientSDK. This allows adding observability features in the ViewModel later on with little change to the Views. You can hook up a View to a Model through the ViewModel; e.g. make the Model a property of the ViewModel. (Direct Binding)
- Put all Persistence Methods, Model transformation logic in ViewModel
- Put non-trivial Model validation rules in ViewModel
- Models should implement INotifyPropertyChanged (inheriting ReactiveObject is one quick way of achiving this)

## When to use Observability 
[Reactive] properties handle two-way binding to Model properties. This is used to bind View component elements to an instance of a Model in a ViewModel. 
If the Component inherits from ReactiveComponentBase<T> then the StateHasChanged() method will be called when the bound values are updated. In otherwords, 
you don't have to do anything special to get two-way binding to work with Models that implment INotifyPropertChanged. 

[ObservableAsProperty] handle read properties that emit events that can be subscribed to by ViewModels and/or Components (both the ViewModel they 
are defined in and other ViewModels). Generally, it is better to implement all the event handling at the ViewModel level for testability and simplicity at the View level.


### Notes
These notes are evolving as we better understand the Reactive Programming Model

- Use Observables to communicate state change inside ViewModels. 
- Use Observables to communicate state change across ViewModels.
- Use [Reactive] properties to communicate simple state change from ViewModel to View.
- Use [ObservableProperty] to comunication complex state change from ViewModel to View.

## Notifications
We use a WebSocket connection to receive notifications from the service. 
The WebSocket is created wen you subscribe to one or more topics. 


