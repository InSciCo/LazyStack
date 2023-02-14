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

public enum StorageAPI
{
    Default,
    Rest, 
    S3,
    Http,
    Local,
    Content // _content 
}
public interface IItemViewModelBase<TModel>
{
    public string UpdateTickField { get; set; }
    public string? Id { get; set; }
    public TModel? Data { get; set; }
    public TModel? DataCopy { get; set; }
    public TModel? NotificationData { get; set; }
    public ItemViewModelBaseState State { get; set; }
    public INotificationEditOption NotificationEditOption { get; set; } 
    public bool NotificationReceived { get; set; }
    public bool CanCreate { get; set; }
    public bool CanRead { get; set; }
    public bool CanUpdate { get; set; }
    public bool CanDelete { get; set; }
    public bool IsLoaded { get; set; }
    public long LastNotificationTick { get; set; }
    public bool IsMerge { get; set; }
    public long UpdateCount { get; set; }
    public bool IsNew { get; }
    public bool IsEdit { get; }
    public bool IsCurrent { get; }
    public bool IsDeleted { get; }

    public Task<(bool, string)> CreateAsync(string? id,StorageAPI storageAPI = StorageAPI.Default);
    public Task<(bool, string)> ReadAsync(string id, StorageAPI storageAPI = StorageAPI.Default);
    public Task<(bool, string)> ReadAsync(StorageAPI storageAPI = StorageAPI.Default);
    public Task<(bool, string)> UpdateAsync(string? id, StorageAPI storageAPI = StorageAPI.Default);
    public Task UpdateFromNotification(string payloadData, string payloadAction, long payloadCreatedAt, long dataUpdatedAt);
    public Task<(bool, string)> SaveEditAsync(string? id, StorageAPI storageAPI = StorageAPI.Default);
    public Task<(bool, string)> DeleteAsync(string id, StorageAPI storageAPI = StorageAPI.Default); 
    public Task<(bool, string)> CancelEditAsync();
}

/// <summary>
/// ItemViewModelBase<T,TEdit> 
/// </summary>
/// <typeparam name="TDTO">DTO Type</typeparam>
/// <typeparam name="TModel">Model Type (extended model off of TDTO)</typeparam>
public class ItemViewModelBase<TDTO, TModel> : LzViewModelBase, IItemViewModelBase<TModel>
    where TDTO : class, new()
    where TModel : class, TDTO, new()
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
    public string UpdateTickField { get; set; } = "UpdatedAt";
    public virtual string? Id { get; set; }
    //{
    //    get { return (Data == null) ? string.Empty : Data.Id; }
    //    set { if (Data != null) Data.Id = value; }
    //}
    [Reactive] public TModel? Data { get; set; }
    [Reactive] public TModel? DataCopy { get; set; }
    [Reactive] public TModel? NotificationData { get; set; }
    [Reactive] public ItemViewModelBaseState State { get; set; }
    public INotificationEditOption NotificationEditOption {get; set; }
    [Reactive] public bool NotificationReceived { get; set; }
    [Reactive] public bool CanCreate { get; set; }
    [Reactive] public bool CanRead { get; set; }
    [Reactive] public bool CanUpdate { get; set; }
    [Reactive] public bool CanDelete { get; set; }
    [Reactive] public bool IsLoaded { get; set; }
    [Reactive] public long LastNotificationTick { get; set; }
    [Reactive] public bool IsMerge { get; set; }
    [Reactive] public virtual long UpdateCount { get; set; }
    [ObservableAsProperty] public bool IsNew { get; }
    [ObservableAsProperty] public bool IsEdit { get; }
    [ObservableAsProperty] public bool IsCurrent { get; }
    [ObservableAsProperty] public bool IsDeleted { get; }
    [Reactive] public StorageAPI StorageAPI { get; set; }   

    // API access - requires authentication
    protected Func<TDTO, Task<TDTO>>? SvcCreateAsync; // Assumes storage Id is in TDTO
    protected Func<string, TDTO, Task<TDTO>>? SvcCreateIdAsync; // Assumes storage Id is passed separate from TDTO
    protected Func<string, Task<TDTO>>? SvcReadIdAsync; 
    protected Func<Task<TDTO>>? SvcReadAsync; // Read using this.Id
    protected Func<TDTO, Task<TDTO>>? SvcUpdateAsync; // Assumes storage Id is in TDTO
    protected Func<string, TDTO, Task<TDTO>>? SvcUpdateIdAsync; // Assumes storage Id is passed separate from TDTO
    protected Func<string, Task>? SvcDeleteIdAsync; 

    // S3 access - requires authentication
    // Id is S3 bucket reference
    protected Func<string, TDTO, Task<TDTO>>? S3SvcCreateIdAsync; 
    protected Func<string, Task<TDTO>>? S3SvcReadIdAsync;
    protected Func<string, TDTO, Task<TDTO>>? S3SvcUpdateIdAsync;
    protected Func<string, Task>? S3SvcDeleteIdAsync;

    // Local storage access
    // Id is full path reference
    protected Func<string, TDTO, Task<TDTO>>? LocalSvcCreateIdAsync;
    protected Func<string, Task<TDTO>>? LocalSvcReadIdAsync;
    protected Func<string, TDTO, Task<TDTO>>? LocalSvcUpdateIdAsync;
    protected Func<string, Task>? LocalSvcDeleteIdAsync;

    // _content access 
    // Id is something like "_content/library/somefile"
    // WASM implements this using HttpClient - assumes resource is under wwwroot
    // MAUI implements this using FileSystem.OpenAppPackageFileAsync(id)
    protected Func<string, Task<TDTO>>? ContentSvcReadIdAsync;

    // Http access - general htpt calls
    // Id is URL
    protected Func<string, Task<TDTO>>? HttpSvcReadIdAsync;

    /// <summary>
    /// This method uses Reflection to look for a long value 
    /// in the field specified by the UpdateTickField property. 
    /// If you don't want the overhead of Reflection or are 
    /// not passing the datetime values as a long, just override 
    /// this method to get the long value of the datetime UTC Ticks
    /// of the lupdate datetime from the data object.
    /// </summary>
    /// <param name="d"></param>
    /// <returns></returns>
    public virtual long ExtractUpdatedTick(object? d)
    {
        if (d is null) 
            return 0;
        if(string.IsNullOrEmpty(UpdateTickField)) 
            return 0;
        Type type = d.GetType();    
        var propertyInfo = type.GetProperty(UpdateTickField);
        if (propertyInfo == null)
            return 0;
        return (long)propertyInfo.GetValue(d, null)!;
    }

    private void CheckAuth(StorageAPI storageAPI)
    {
        // Check for Auth
        switch (storageAPI)
        {
            case StorageAPI.Rest:
            case StorageAPI.S3:
                if (AuthProcess == null)
                    throw new Exception("AuthProcess not assigned");

                if (AuthProcess.IsNotSignedIn)
                    throw new Exception("Not signed in.");
                break;
            default:
                break;
        }
    }

    private void CheckId(string? id)
    {
        if (id is null)
            throw new Exception("Id is null");
    }

    public virtual async Task<(bool, string)> CreateAsync(string? id, StorageAPI storageAPI = StorageAPI.Default)
    {
        if (storageAPI == StorageAPI.Default)
            storageAPI = (StorageAPI == StorageAPI.Default) 
                ? StorageAPI.Rest
                : StorageAPI;

        try
        {
            if (!CanCreate)
                throw new Exception("Create not authorized");

            if (State != ItemViewModelBaseState.New)
                throw new Exception("State != New");

            if (Data == null)
                throw new Exception("Data not assigned");

            var item = (TDTO)Data;

            if (!Validate())
                throw new Exception("Validation failed.");

            CheckAuth(storageAPI);

            // Perform storage operation
            switch (storageAPI)
            {
                case StorageAPI.Rest:
                    if (id is null)
                    {
                        if (SvcCreateAsync == null)
                            throw new Exception("SvcCreateAsync not assigned.");
                        item = await SvcCreateAsync(item!);
                    }
                    else
                    {
                        if (SvcCreateIdAsync == null)
                            throw new Exception("SvcCreateIdAsync not assigned.");
                        CheckId(id);
                        item = await SvcCreateIdAsync(id!, item!);
                    }
                    break;
                case StorageAPI.S3:
                    if (S3SvcCreateIdAsync == null)
                        throw new Exception("S3SvcCreateIdAsync not assigned.");
                    CheckId(id);
                    item = await S3SvcCreateIdAsync(id!, item!);
                    break;
                case StorageAPI.Http:
                    throw new Exception("HttpSvcCreateIdAsync is not supported.");
                case StorageAPI.Content:
                    throw new Exception("ContentSvcCreateIdAsync is not supported.");
                case StorageAPI.Local:
                    if (LocalSvcCreateIdAsync == null)
                        throw new Exception("LocalSvcCreateIdAsync not assigned.");
                    CheckId(id);
                    item = await LocalSvcCreateIdAsync(id!,item!);
                    break;   
            }

            UpdateData(item);
            State = ItemViewModelBaseState.Current;
            return (true, string.Empty);
        }
        catch (Exception ex)
        {
            return (false, Log(MethodBase.GetCurrentMethod()!, ex.Message));
        }
    }
    public virtual async Task<(bool, string)> ReadAsync(string id, StorageAPI storageAPI = StorageAPI.Default)
    {
        if (storageAPI == StorageAPI.Default)
            storageAPI = (StorageAPI == StorageAPI.Default)
                ? StorageAPI.Rest
                : StorageAPI;
        try
        {
            if (!CanRead) 
                throw new Exception("Read not authorized");

            CheckAuth(storageAPI);
            CheckId(id);

            // Perform storage operation
            switch (storageAPI)
            {
                case StorageAPI.Rest:
                    if (SvcReadIdAsync == null)
                        throw new Exception("SvcReadIdAsync not assigned.");
                    UpdateData(await SvcReadIdAsync(id));
                    break;
                case StorageAPI.S3:
                    if (S3SvcReadIdAsync == null)
                        throw new Exception("S3SvcReadIdAsync not assigned.");
                    UpdateData(await S3SvcReadIdAsync(id));
                    break;
                case StorageAPI.Http:
                    if (HttpSvcReadIdAsync == null)
                        throw new Exception("HttpSvcReadIdAsync not assigned.");
                    UpdateData(await HttpSvcReadIdAsync(id));
                    break;
                case StorageAPI.Content:
                    if (ContentSvcReadIdAsync == null)
                        throw new Exception("ContentSvcReadIdAsync not assigned.");
                    UpdateData(await ContentSvcReadIdAsync(id));
                    break;
                case StorageAPI.Local:
                    if (LocalSvcReadIdAsync == null)
                        throw new Exception("LocalSvcReadIdAsync not assigned.");
                    UpdateData(await LocalSvcReadIdAsync(id));
                    break;
                
            }
            
            LastNotificationTick = ExtractUpdatedTick(Data);
            State = ItemViewModelBaseState.Current;
            return (true, string.Empty);
        }
        catch (Exception ex)
        {
            return (false, Log(MethodBase.GetCurrentMethod()!, ex.Message));
        }
    }
    // ReadAsync without an Id is used to read from an API, often where  the API uses the 
    // logged in identity of the caller as an id for data retrieval. The identity of the 
    // caller is contained in the JWT or Authentication Signature so as to make it 
    // impossible for a sniffer to see the id of the data requested. Currently, this is 
    // relevant to the Rest API but we may extend it to the S3 service as well.
    public virtual async Task<(bool, string)> ReadAsync(StorageAPI storageAPI = StorageAPI.Default)
    {
        if (storageAPI == StorageAPI.Default)
            storageAPI = (StorageAPI == StorageAPI.Default)
                ? StorageAPI.Rest
                : StorageAPI;
        try
        {
            if (!CanRead)
                throw new Exception("Read not authorized");

            CheckAuth(storageAPI);

            // Perform storage operation
            switch (storageAPI)
            {
                case StorageAPI.Rest:
                    if (SvcReadAsync == null)
                        throw new Exception("SvcReadAsync not assigned.");
                    UpdateData(await SvcReadAsync());
                    break;
                case StorageAPI.S3:
                    throw new Exception("S3SvcReadAsync not supported. Use S3SvcReadIdAsync instead.");
                case StorageAPI.Http:
                    throw new Exception("HttpSvcReadAsync not supported. Use HttpSvcReadIdAsync instead.");
                case StorageAPI.Content:
                    throw new Exception("ContentSvcReadAsync not supported. Use ContentSvcReadIdAsync instead.");
                case StorageAPI.Local:
                    throw new Exception("LocalSvcReadAsync not supported. Use LocalSvcReadIdAsync instead.");
            }
            LastNotificationTick = ExtractUpdatedTick(Data);
            // Id = Data!.Id;
            State = ItemViewModelBaseState.Current;
            return (true, string.Empty);
        }
        catch (Exception ex)
        {
            return (false, Log(MethodBase.GetCurrentMethod()!, ex.Message));
        }
    }
    public virtual async Task<(bool, string)> UpdateAsync(string? id, StorageAPI storageAPI = StorageAPI.Default)
    {
        if (storageAPI == StorageAPI.Default)
            storageAPI = (StorageAPI == StorageAPI.Default)
                ? StorageAPI.Rest
                : StorageAPI;

        try
        {
            if (!CanUpdate) 
                throw new Exception("Update not autorized");

            if (State != ItemViewModelBaseState.Edit)
                throw new Exception("State != Edit.");

            if (Data is null)
                throw new Exception("Data not assigned");

            if (!Validate())
                throw new Exception("Validation failed.");

            CheckAuth(storageAPI);

            switch(storageAPI)
            {
                case StorageAPI.Rest:
                    if(id is null)
                    {
                        if (SvcUpdateAsync == null)
                            throw new Exception("SvcUpdateAsync is not assigned.");
                        UpdateData(await SvcUpdateAsync((TDTO)Data!));
                    }
                    else
                    {
                        if (SvcUpdateIdAsync == null)
                            throw new Exception("SvcUpdateIdAsync is not assigned.");
                        CheckId(id);
                        UpdateData(await SvcUpdateIdAsync(id,(TDTO)Data!));
                    }
                    break;
                case StorageAPI.S3:
                    if (S3SvcUpdateIdAsync == null)
                        throw new Exception("S3SvcUpdateIdAsync is not assigned.");
                    CheckId(id);
                    UpdateData(await S3SvcUpdateIdAsync(id!, (TDTO)Data!));
                    break;
                case StorageAPI.Http:
                    throw new Exception("HttpSvcUpdateIdAsync is not supported.");
                case StorageAPI.Content:
                    throw new Exception("ContentSvcUpdateIdAsync is not supported.");
                case StorageAPI.Local:
                    if (LocalSvcUpdateIdAsync == null)
                        throw new Exception("LocalSvcUpdateIdAsync is not assigned.");
                    CheckId(id);
                    UpdateData(await LocalSvcUpdateIdAsync(id!,(TDTO)Data!));
                    break;
            }

            LastNotificationTick = ExtractUpdatedTick(Data);
            State = ItemViewModelBaseState.Current;
            return (true, string.Empty);
        }
        catch (Exception ex)
        {
            return (false, Log(MethodBase.GetCurrentMethod()!, ex.Message));
        }
    }
    public virtual async Task UpdateFromNotification(string payloadData, string payloadAction, long payloadCreatedAt, long dataUpdatedAt)
    {
        /*
            LastNotificationTick holds the datetime of the last read or notification processed 
            payloadCreatedAt holds the datetime of the update time of the payload contained in the notificiation 
            dataUpdatedAt is the updated datetime of the current data 
        */
        Console.WriteLine($"Item Notification: {this.GetType()} {payloadAction} {payloadData}");
        Console.WriteLine($"     payloadCreatedAt: {payloadCreatedAt}");
        Console.WriteLine($"        dataUpdatedAt: {dataUpdatedAt}");
        Console.WriteLine($"LastNotificationsTick: {LastNotificationTick}");
        Console.WriteLine($"payLoadCreatedAt - dataUpdatedAt {payloadCreatedAt - dataUpdatedAt}");

        if (State != ItemViewModelBaseState.Edit && State != ItemViewModelBaseState.Current)
            return;

        if (payloadCreatedAt <= LastNotificationTick)
        {
            Console.WriteLine("skipping: payLoadCreatedAt <= LastNotificationsTick");
            return;
        }

        LastNotificationTick = payloadCreatedAt;

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
                UpdateData(dataObj);
                LastNotificationTick = ExtractUpdatedTick(Data);
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
                        UpdateData(dataObj);
                        LastNotificationTick = ExtractUpdatedTick(Data);
                        await OpenEditAsync();
                        break;

                    case INotificationEditOption.Merge:
                        Console.WriteLine("State == Edit && Action == Merge");
                        UpdateData(dataObj);    
                        LastNotificationTick = ExtractUpdatedTick(Data);
                        IsMerge = true;
                        break;
                    default:
                        return;

                }
            }
        } catch 
        { 
            // Todo: What, if anything, do we want to do here? Maybe just log?
        }
        finally
        {
            UpdateCount++;
        }
    }

    public virtual async Task<(bool,string)> DeleteAsync(string id, StorageAPI storageAPI = StorageAPI.Default)
    {
        if (storageAPI == StorageAPI.Default)
            storageAPI = (StorageAPI == StorageAPI.Default)
                ? StorageAPI.Rest
                : StorageAPI;

        try
        {
            if (!CanDelete)
                throw new Exception("Delete(id) not autorized.");

            if (State != ItemViewModelBaseState.Current)
                throw new Exception("State != Current");

            CheckAuth(storageAPI);
            CheckId(id); 
            switch(storageAPI)
            {
                case StorageAPI.Rest:
                    if (SvcDeleteIdAsync == null)
                        throw new Exception("SvcDelete(id) is not assigned.");
                    await SvcDeleteIdAsync(Id!);
                    break;
                case StorageAPI.S3:
                    if (S3SvcDeleteIdAsync == null)
                        throw new Exception("S3SvcDelete(id) is not assigned.");
                    await S3SvcDeleteIdAsync(Id!);
                    break;
                case StorageAPI.Http:
                    throw new Exception("HttpSvcDelete(id) is not supported.");
                case StorageAPI.Content:
                    throw new Exception("ContentSvcDelete(id) is not supported.");
                case StorageAPI.Local:
                    if (LocalSvcDeleteIdAsync == null)
                        throw new Exception("LocalSvcDelete(id) is not assigned.");
                    await LocalSvcDeleteIdAsync(Id!);
                    break;
            }

            State = ItemViewModelBaseState.Deleted;
            Data = null;
            return(true,String.Empty);

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
    public virtual async Task<(bool,string)> SaveEditAsync(string? id, StorageAPI storageAPI = StorageAPI.Default)
    {
        try
        {
            var (success, msg) =
                State == ItemViewModelBaseState.New
                ? await CreateAsync(id, storageAPI)
                : await UpdateAsync(id, storageAPI);

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

    protected void UpdateData(TDTO item)
    {
        item.DeepCloneTo(Data!);
        this.RaisePropertyChanged(nameof(Data));
        
    }
}
