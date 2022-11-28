using System.Reflection;
using ReactiveUI.Fody.Helpers;
using Force.DeepCloner;
using LazyStackAuthV2;
using ReactiveUI;
using Newtonsoft.Json;

namespace LazyStack.ViewModels;
/*
Notifications 
This ViewModel class supports INotificationSvc by providing:
    UpdateFromNotification(string data) -- TDTO object in JSON form
    NotificationEditOption = Cancel | Merge 
    TModel DataCopy
    TModel NotificationData - data updated from most recent notification if State == Edit
    [Reactive] bool NotificationReceived - fired when a notification recieved

The INotificationSvc receives Notification updates from a service (either by polling or websocket).
Notification
    Id - a GUID for each notification
    TopicId - we subscribe to topics in the INotificationSvc - this selects what notifications we receive from the service
    UserId - not currently used
    PayloadParentId - usually the Id field of the payload items parent - normally used by ItemsViewModel subscription
    PayloadId - usually the Id field from the payload class - normally used by ItemViewModel subscription
    PayloadType - The name of the class serialized into the Payload data
    Payload  - JSON string containing serialized instance of PayloadType
    PayloadAction - Create, Update, Delete. Note that only the Update action is meaningful for the ItemViewModel
                    Create and Delete are generally handled by the ItemsViewModel owning the ItemViewModel instances.
    CreatedAt - datetime utc ticks (store as long)

- We filter on Data.Id 
- We process Notification Actions to update the ViewModel Data.

What we do with a Notification depends on the current state of the ViewModel. 
State == New
    - Notification should not pass filter as the new Data has no Id
    - Note that the ItemsViewModel normally handles adding ItemViewModel instances
State == Current 
    - We update the ViewModel Data
State == Deleted
    - Ignore
    - The viewmodel is probably being disposed of when the notification arrives
State == Edit 
    This one is complicated and we use NotificationEditOption to govern behavior:
    NotificationEditOption == Cancel 
        - We cancel the current edit and update the ViewModel Data from the Notification
    NotificationEditOption == Merge 
        - We maintain three buffers 
            - DataCopy, a copy of the data as it was before edit 
            - NotificationBuffer, a copy of the data as updated by the last processed Notification 
            - Data, the currently edited data 
        - We maintain three ChangeLists Dictionary<propertyname, propertyvalue)
            - NotificationChanges: DataCopy propertyvalue <> NotificationData propertyvalue 
            - EditChanges: DataCopy propertyvalue <> Data propertyvalue 
            - Conflicts: EditChanges propertyvalue <> Data propertyvalue 
        - We let the UI do whatever it wants with this information. Example:
            - Selectively update the Data properties from NotificationChanges and EditChanges into Data
            - Update the UpdatedAt timestamp in Data from NotificationChanges so your SaveEditAsync() doesn't fail *** VERY IMPORTANT ***
            - Finalize the Edit 

** Effect of Notification on in-flight updates. **
When we finalize an Edit, we call SaveEditAsync(). If the State == Edit then this is calling an 
update on the service side. We use optimistic locking on the service side so it is possible that 
the update may fail if another client updated the record while we were editing it in our client.

If the notification created by the other client's edit arrives before we finalize our edit then 
the NotificationEditOption process (Cancel or Merge) kicks in. 

However, what happens if the Notification arrives after we Finalize our Edit (which makes the 
SaveEditAsymc() call) to do an update on the service side? We wait for the update to fail, 
swallow the error and proceed based on the NotificationEditOption.


** Using Notifications ** 
Add and initialize INotificationSvc NotificationSvc property to your implementing ViewModel. 
In your constructor, add a subscription:
    this.WhenAnyValue(x => x.NotificationSvc.Notification)
        .Where(x => x.PayloadId.Equals(Id)) // Remember we reflect Data.Id to ViewModel.Id by default
        .Subscribe(x => UpdateFromNotification(x.Payload));

 */


public enum ItemViewModelBaseState
{
    New,
    Edit,
    Current,
    Deleted
}

public enum INotificationEditOption
{
    Cancel, // default
    Merge
}

public interface IItemViewModelBase<TModel>
{
    public string? Id { get; set; }
    public TModel? Data { get; set; }
    public TModel? DataCopy { get; set; }
    public TModel? NotificationData { get; set; }
    public ItemViewModelBaseState State { get; set; }
    public INotificationEditOption NotificationEditOption { get; set; } 
    public bool NotificationReceived { get; set; }  
   
    public Task<(bool, string)> CreateAsync();
    public Task<(bool, string)> ReadAsync(string id);
    public Task<(bool, string)> ReadAsync();
    public Task<(bool, string)> UpdateAsync();
    public Task UpdateFromNotification(string payloadData, string payloadAction, long payloadCreatedAt, long dataUpdatedAt);
    public Task<(bool, string)> SaveEditAsync();
    public Task<(bool, string)> DeleteAsync(); 
    public Task<(bool, string)> CancelEditAsync();
    public bool CanCreate { get; set; }
    public bool CanRead { get; set; }   
    public bool CanUpdate { get; set; }    
    public bool CanDelete { get; set; } 
    public bool IsLoaded { get; set; }  
    public bool IsNew { get; }
    public bool IsEdit { get; }
    public bool IsCurrent { get; }
    public bool IsDeleted { get; }


}

/// <summary>
/// ItemViewModelBase<T,TEdit> 
/// </summary>
/// <typeparam name="TDTO">DTO Type</typeparam>
/// <typeparam name="TModel">Model Type (extended model off of TDTO)</typeparam>
/// <typeparam name="TParent">ParentViewModel Type</typeparam>
public class ItemViewModelBase<TDTO, TModel> : LzViewModelBase, IItemViewModelBase<TModel>
    where TDTO : class, new()
    where TModel : class, TDTO, IId, new()
{
    public ItemViewModelBase()
    {
        CanCreate = true;
        CanRead = true;
        CanUpdate = true;
        CanDelete = true;
        IsLoaded = false;
        // ActiveEdit = false;

        this.WhenAnyValue(x => x.State, (x) => x == ItemViewModelBaseState.New)
            .ToPropertyEx(this, x => x.IsNew);

        this.WhenAnyValue(x => x.State, (x) => x == ItemViewModelBaseState.Edit)
            .ToPropertyEx(this, x => x.IsEdit);

        this.WhenAnyValue(x => x.State, (x) => x == ItemViewModelBaseState.Current)
            .ToPropertyEx(this, x => x.IsCurrent);

        this.WhenAnyValue(x => x.State, (x) => x == ItemViewModelBaseState.Deleted)
            .ToPropertyEx(this, x => x.IsDeleted);

    }

    public IAuthProcess? AuthProcess { get; set; }
    [Reactive] public TModel? Data { get; set; }
    [Reactive] public TModel? DataCopy { get; set; }
    [Reactive] public TModel? NotificationData { get; set; }
    [Reactive] public ItemViewModelBaseState State { get; set; }
    public INotificationEditOption NotificationEditOption {get; set; }
    [Reactive] public bool NotificationReceived { get; set; }
    public virtual string? Id
    {
        get { return (Data == null) ? string.Empty : Data.Id; }
        set { if(Data != null) Data.Id = value; }
    }

    protected Func<TDTO, Task<TDTO>>? SvcCreateAsync;
    protected Func<string, Task<TDTO>>? SvcReadIdAsync;
    protected Func<Task<TDTO>>? SvcReadAsync;
    protected Func<TDTO, Task<TDTO>>? SvcUpdateAsync;
    protected Func<string, Task>? SvcDeleteIdAsync;
    protected Func<Task<TDTO>>? SvcDeleteAsync;

    [Reactive] public bool CanCreate { get; set; }
    [Reactive] public bool CanRead { get; set; }
    [Reactive] public bool CanUpdate { get; set; }
    [Reactive] public bool CanDelete { get; set; }
    [Reactive] public bool IsLoaded { get; set; }
    [Reactive] public long LastNotificationTick { get; set; }
    [Reactive] public bool IsMerge { get; set; }
    [ObservableAsProperty] public bool IsNew { get; }
    [ObservableAsProperty] public bool IsEdit { get; }
    [ObservableAsProperty] public bool IsCurrent { get; }
    [ObservableAsProperty] public bool IsDeleted { get; }

    public virtual async Task<(bool, string)> CreateAsync()
    {
        try
        {
            if (!CanCreate)
                throw new Exception("Create not authorized");

            if (AuthProcess == null)
                throw new Exception("AuthProcess not assigned");

            if (AuthProcess.IsNotSignedIn)
                throw new Exception("Not signed in.");

            if (!Validate())
                throw new Exception("Validation failed.");

            if (State != ItemViewModelBaseState.New)
                throw new Exception("State != New.");

            if (SvcCreateAsync == null)
                throw new Exception("SvcCreateAsync not assigned.");

            if (Data == null)
                throw new Exception("Data not assigned");

            var item = (TDTO)Data; 

            item = await SvcCreateAsync(item!);
            item.DeepCloneTo(Data);

            State = ItemViewModelBaseState.Current;
            return (true, string.Empty);
        }
        catch (Exception ex)
        {
            return (false, Log(MethodBase.GetCurrentMethod()!, ex.Message));
        }
    }
    public virtual async Task<(bool, string)> ReadAsync(string id)
    {
        try
        {
            if (!CanRead) 
                throw new Exception("Read not authorized");

            if (AuthProcess == null)
                throw new Exception("AuthProcess not assigned");

            if (AuthProcess.IsNotSignedIn)
                throw new Exception("Not signed in.");

            if (SvcReadIdAsync == null)
                throw new Exception("SvcReadAsync not assigned.");

            var item = await SvcReadIdAsync(id);
            item.DeepCloneTo(Data!);
            Id = id;
            State = ItemViewModelBaseState.Current;
            return (true, string.Empty);
        }
        catch (Exception ex)
        {
            return (false, Log(MethodBase.GetCurrentMethod()!, ex.Message));
        }
    }
    public virtual async Task<(bool, string)> ReadAsync()
    {
        try
        {
            if (!CanRead)
                throw new Exception("Read not authorized.");

            if (AuthProcess == null)
                throw new Exception("AuthProcess not assigned");

            if (AuthProcess.IsNotSignedIn)
                throw new Exception("Not signed in.");

            if (SvcReadAsync == null)
                throw new Exception("SvcReadAsync not assigned.");


            var item = await SvcReadAsync();
            item.DeepCloneTo(Data!);
            Id = Data!.Id;
            State = ItemViewModelBaseState.Current;
            return (true, string.Empty);
        }
        catch (Exception ex)
        {
            return (false, Log(MethodBase.GetCurrentMethod()!, ex.Message));
        }
    }
    public virtual async Task<(bool, string)> UpdateAsync()
    {
        try
        {
            if (!CanUpdate) 
                throw new Exception("Update not autorized");

            if (AuthProcess == null)
                throw new Exception("AuthProcess not assigned");

            if (AuthProcess.IsNotSignedIn)
                throw new Exception("Not signed in.");

            if (!Validate())
                throw new Exception("Validation failed.");

            if (State != ItemViewModelBaseState.Edit)
                throw new Exception("State != Edit.");

            if (SvcUpdateAsync == null)
                throw new Exception("SvcUpdateAsync is not assigned.");

            if (Data is null)
                throw new Exception("Data not assigned");

            var item = (TDTO)Data!;
            item = await SvcUpdateAsync(item);
            item.DeepCloneTo(Data!);
            State = ItemViewModelBaseState.Current;
            return (true, string.Empty);
        }
        catch (Exception ex)
        {
            return (false, Log(MethodBase.GetCurrentMethod()!, ex.Message));
        }
    }
    public virtual async Task UpdateFromNotification(string payloadData, string payloadAction, long payLoadCreatedAt, long dataUpdatedAt)
    {
        /*
            
        */
        Console.WriteLine($"Item Notification: {this.GetType()} {payloadAction} {payloadData}");

        if (State != ItemViewModelBaseState.Edit && State != ItemViewModelBaseState.Current)
            return;

        if (payLoadCreatedAt <= LastNotificationTick)
        {
            Console.WriteLine("skipping: payLoadCreatedAt <= LastNotificationsTick");
            return;
        }

        if (dataUpdatedAt >= LastNotificationTick)
        {
            Console.WriteLine("skipping: dataUpdatedAt >= LastNotificationTick");
            return;
        }

        LastNotificationTick = payLoadCreatedAt;

        try
        {
            var dataObj = JsonConvert.DeserializeObject<TDTO>(payloadData);
            if (dataObj == null)
            {
                Console.WriteLine("dataObj is null");
                return;
            }

            if(State == ItemViewModelBaseState.Current)
            {
                if (payloadAction.Equals("Delete"))
                {
                    Console.WriteLine("State == Current && Action == Delete - not handled");
                    return; // this action is handled at the ItemsViewModel level
                }
                dataObj.DeepCloneTo(Data!);
                NotificationReceived = true; // Fires off event in case we want to inform the user an update occurred
                Console.WriteLine("Data object updated from dataObj");
                return;
            }

            if(State == ItemViewModelBaseState.Edit)
            {
                if(payloadAction.Equals("Delete"))
                {
                    Console.WriteLine("State == Edit && Action == Delete");
                    await CancelEditAsync();
                    return; // The actual delete is handled at the ItemsViewModel level
                }

                switch(NotificationEditOption)
                {
                    case INotificationEditOption.Cancel:
                        Console.WriteLine("State == Edit && Action == Cancel");
                        await CancelEditAsync();
                        dataObj.DeepCloneTo(Data!);
                        await OpenEditAsync();
                        break;

                    case INotificationEditOption.Merge:
                        Console.WriteLine("State == Edit && Action == Merge");
                        dataObj.DeepCloneTo(NotificationData!);
                        IsMerge = true;
                        break;
                    default:
                        return;

                }
            }
        } catch (Exception ex) 
        { 
        
        }
    }
    public virtual (bool, string) UpdateFromJson(string json)
    {
        try
        {
            var item = JsonConvert.DeserializeObject<TDTO>(json);
            item.DeepCloneTo(Data!);
            return (true, string.Empty);
        }
        catch (Exception ex)
        {
            return (false, Log(MethodBase.GetCurrentMethod()!, ex.Message));
        }
    }

    public virtual async Task<(bool,string)> DeleteAsync(string Id)
    {
        try
        {
            if (!CanDelete)
                throw new Exception("Delete not autorized.");

            if (AuthProcess == null)
                throw new Exception("AuthProcess not assigned");

            if (AuthProcess.IsNotSignedIn)
                throw new Exception("Not signed in.");

            if (State != ItemViewModelBaseState.Current)
                throw new Exception("State != Current");

            if (SvcDeleteIdAsync == null)
                throw new Exception("SvcDelete is not assigned.");

            await SvcDeleteIdAsync(Id);
            State = ItemViewModelBaseState.Deleted;
            Data = null;
            return(true,String.Empty);

        }
        catch (Exception ex)
        {
            return (false, Log(MethodBase.GetCurrentMethod()!, ex.Message));
        }
    }
    public virtual async Task<(bool, string)> DeleteAsync()
    {
        try
        {
            if (!CanDelete) 
                throw new Exception("Delete not authorized");

            if (AuthProcess == null)
                throw new Exception("AuthProcess not assigned");

            if (AuthProcess.IsNotSignedIn)
                throw new Exception("Not signed in.");

            if (State != ItemViewModelBaseState.Current)
                throw new Exception("State != Current");

            if (SvcDeleteAsync == null)
                throw new Exception("SvcDelete is not assigned.");

            await SvcDeleteAsync();
            State = ItemViewModelBaseState.Deleted;
            Data = null;
            return (true, String.Empty);

        }
        catch (Exception ex)
        {
            return (false, Log(MethodBase.GetCurrentMethod()!, ex.Message));
        }
    }
    public virtual Task OpenEditAsync()
    {
        if(State != ItemViewModelBaseState.New)
            State = ItemViewModelBaseState.Edit;
        DataCopy ??= new();
        Data.DeepCloneTo(DataCopy);
        return Task.CompletedTask;
    }
    public virtual async Task<(bool,string)> SaveEditAsync()
    {
        try
        {
            var (success, msg) =
                State == ItemViewModelBaseState.New
                ? await CreateAsync()
                : await UpdateAsync();

            State = ItemViewModelBaseState.Current;
            IsMerge = false;
            IsLoaded = true;

            return (success, msg);
        } 
        catch (Exception ex)
        {
            return (false, Log(MethodBase.GetCurrentMethod()!, ex.Message));
        }
    }
    public virtual async Task<(bool,string)> CancelEditAsync()
    {
        await Task.Delay(0);
        if (State != ItemViewModelBaseState.Edit && State != ItemViewModelBaseState.New)
            return (false, Log(MethodBase.GetCurrentMethod()!, "No Active Edit"));

        State = (IsLoaded) ? ItemViewModelBaseState.Current : ItemViewModelBaseState.New;

        if (IsMerge)
            NotificationData.DeepCloneTo(Data);
        else
            DataCopy.DeepCloneTo(Data);
        IsMerge= false;
        return (true,String.Empty);
    }
    public virtual bool Validate()
    {
        return true;
    }

}
