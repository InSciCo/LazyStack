using ReactiveUI.Fody.Helpers;
using ReactiveUI;
using System.Collections.Specialized;

namespace LazyStack.ViewModels;
/*
Notifications
This ViewModel class supports INotifications by providing:
    UpdateFromNotification(string data, string action) -- TDTO object in JSON, Action = Create | Delete
    [Reactive] bool NotificationReceived - fired when notification received

The INotificationSvc receives Notification updates from a service (either by polling or websocekt).
Notification:
    Id - a GUID for each notification
    TopicId - we subscribe to topics in the INotificationSvc - this selects what notifications we receive from the service
    UserId - not currently used
    PayloadParentId - usually the Id field of the payload items parent - normally used by ItemsViewModel subscription
    PayloadId - usually the Id field from the payload class - normally used by ItemViewModel subscription
    PayloadType - The name of the class serialized into the Payload data
    Payload  - JSON string containing serialized instance of PayloadType
    PayloadAction - Create, Update, Delete. Create and Delete are generally handled by ItemsViewModel. Update is 
                    generally handled by the ItemViewModel.
    CreatedAt - datetime utc ticks (store as long)
  
** Using Notifications **
Add and initialize INotificationSvc NotificationSvc property to your implementing ViewModel. 
Add a ParentId property to your ViewModel. Usually something like string ParentId => ParentViewModel.Id.
In your constructor, add a subscription:
    this.WhenAnyValue(x => x.NotificationSvc.Notification)
        .Where(x => x.PayloadParentId.Equals(ParentId) && x.PayloadAction.Equals("Create"))
        .Subscribe(x => CreateFromNotification(new viewModel(this, JsonConvert.DeserializeObject<datatype>(x.Payload));

    this.WhenAnyValue(x => x.NotificationSvc.Notification)
        .Where(x => x.PayloadParentId.Equals(ParentId) && x.PayloadAction.Equals("Delete"))
        .Subscribe(x => DeleteFromNotification(x.PayloadId);

*/

/// <summary>
/// This class manages a list of ViewModels
/// TVM is the ViewModel Class in the list
/// </summary>
/// <typeparam name="TVM"></typeparam>
public class LzItemsViewModelBase<TVM> : LzViewModelBase, INotifyCollectionChanged
    where TVM : class, IItemViewModelBase
{
    public LzItemsViewModelBase()
    {
        CanList = true;
        CanAdd = true;
    }

    public string? Id { get; set; }
    public Dictionary<string, TVM> ViewModels { get; set; } = new();

    private TVM? currentViewModel;
    public TVM? CurrentViewModel 
    { 
        get { return currentViewModel; }
        set
        {
            if (value != null && value != LastViewModel && value!.State != ItemViewModelBaseState.New)
                LastViewModel = value;
            this.RaiseAndSetIfChanged(ref currentViewModel, value);
        }
    }
    [Reactive] public TVM? LastViewModel { get; set; }
    protected int changeCount;

    public event NotifyCollectionChangedEventHandler? CollectionChanged;
    public bool IsChanged 
    {
        get { return changeCount > 0; }
        set 
        {
            var newVal = changeCount + 1;
            this.RaiseAndSetIfChanged(ref changeCount, newVal);
        } 
    }
    [Reactive] public bool IsLoaded { get; set; }
    [Reactive] public long LastLoadTick { get; set; }
    [Reactive] public bool CanList { get; set; }
    [Reactive] public bool CanAdd { get; set; }
    [Reactive] public long NotificationLastTick { get; set; }
    [Reactive] public virtual long UpdateCount { get; set; }

    public virtual async Task<(bool,string)> CancelCurrentViewModelEditAsync()
    {
        try 
        { 
            if (CurrentViewModel == null)
                return (false, "CurrentViewModel is null");

            if(CurrentViewModel.State != ItemViewModelBaseState.New && CurrentViewModel.State != ItemViewModelBaseState.Edit)
                throw new Exception("State != Edit && State != New");

            await CurrentViewModel.CancelEditAsync();

            if (CurrentViewModel.State == ItemViewModelBaseState.New)
            {
                if (LastViewModel?.Id != null && ViewModels.ContainsKey(LastViewModel.Id!))
                    CurrentViewModel = LastViewModel;
                else
                    CurrentViewModel = null;
                return (true,string.Empty);
            }

            return (true, String.Empty);
        }
        catch (Exception ex)
        {
            return (false, Log(string.Empty, ex.Message));
        }
    }


    public virtual async Task<(bool,string)> SaveCurrentViewModelAsync(string? id)
    {
        if (CurrentViewModel == null)
            return (false, "CurrentViewModel is null");

        var isAdd = CurrentViewModel.State == ItemViewModelBaseState.New;
        var (success, msg) = await CurrentViewModel.SaveEditAsync(id);
        if (success && isAdd)
        {
            if (CurrentViewModel.Id == null)
                throw new Exception("ItemViewModel.Id is null");
            ViewModels.TryAdd(CurrentViewModel.Id, CurrentViewModel);
        }

        return (success,msg);
    }


    public virtual async Task DeleteFromNotification(string payloadId)
    {
        if(ViewModels.TryGetValue(payloadId, out var vm))
        {
            await vm.DeleteAsync(payloadId); // Gives the ItemViewModel a chance to inform the UI that a delete is taking place
            ViewModels.Remove(payloadId);   
        }
        await Task.Delay(0);
    }

    public virtual async Task CreateFromNotification(TVM vm)
    {
        if(!ViewModels.ContainsKey(vm.Id!))
            ViewModels.Add(vm.Id!, vm);
        await Task.Delay(0);
    }

}
