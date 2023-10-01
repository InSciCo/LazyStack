namespace LazyStack.ViewModels;

/// <summary>
/// ItemViewModelBase<T,TEdit>
/// Remember to call base() constructor to get subscriptions and state set properly.
/// </summary>
/// <typeparam name="TDTO">DTO Type</typeparam>
/// <typeparam name="TModel">Model Type (extended model off of TDTO)</typeparam>
public abstract class LzItemViewModelBase<TDTO, TModel> : LzViewModelBase, ILzItemViewModelBase, ILzItemViewModelBaseData<TModel>
    where TDTO : class, new()
    where TModel : class, TDTO, IRegisterObservables, new()
{
    public LzItemViewModelBase(TDTO item, bool? isLoaded = null)
    {
        CanCreate = true;
        CanRead = true;
        CanUpdate = true;
        CanDelete = true;
        IsLoaded = false;
        IsDirty = false;

        this.WhenAnyValue(x => x.State, (x) => x == LzItemViewModelBaseState.New)
            .ToPropertyEx(this, x => x.IsNew);

        this.WhenAnyValue(x => x.State, (x) => x == LzItemViewModelBaseState.Edit)
            .ToPropertyEx(this, x => x.IsEdit);

        this.WhenAnyValue(x => x.State, (x) => x == LzItemViewModelBaseState.Current)
            .ToPropertyEx(this, x => x.IsCurrent);

        this.WhenAnyValue(x => x.State, (x) => x == LzItemViewModelBaseState.Deleted)
            .ToPropertyEx(this, x => x.IsDeleted);

        // Init Model Data 
        if (item != null)
            item.DeepCloneTo(Data = new());
        State = (Data == null) ? LzItemViewModelBaseState.New : LzItemViewModelBaseState.Current;
        IsLoaded = isLoaded ??= Data != null;
        Data ??= new();
        Data.RegisterObservables();

    }
    protected string entityName = string.Empty; 
    public IAuthProcess? AuthProcess { get; set; }
    public string UpdateTickField { get; set; } = "UpdatedAt";
    public bool AutoLoadChildren { get; set; } = true;
    public abstract string? Id { get; }
    public abstract long UpdatedAt { get; }

    public string dataCopyJson = string.Empty;
    [Reactive] public TModel? Data { get; set; }
    //[Reactive] public TModel? DataCopy { get; set; }
    [Reactive] public TModel? NotificationData { get; set; }
    [Reactive] public LzItemViewModelBaseState State { get; set; }

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
    [Reactive] public bool IsDirty { get; set; }
    [Reactive] public StorageAPI StorageAPI { get; set; }

    public ILzParentViewModel? ParentViewModel { get; set; }

    // API access - requires authentication
    public Func<TDTO, Task<TDTO>>? SvcCreateAsync { get; init; } // Assumes storage Id is in TDTO
    public Func<string, TDTO, Task<TDTO>>? SvcCreateIdAsync { get; init; } // Assumes storage Id is passed separate from TDTO
    public Func<string, Task<TDTO>>? SvcReadIdAsync { get; init; }
    public Func<Task<TDTO>>? SvcReadAsync { get; init; } // Read using this.Id
    public Func<TDTO, Task<TDTO>>? SvcUpdateAsync { get; init; } // Assumes storage Id is in TDTO
    public Func<string, TDTO, Task<TDTO>>? SvcUpdateIdAsync { get; init; } // Assumes storage Id is passed separate from TDTO
    public Func<string, Task>? SvcDeleteIdAsync { get; init; }
    // S3 access - requires authentication
    // Id is S3 bucket reference
    public Func<string, string, Task>? S3SvcCreateIdAsync { get; init; }
    public Func<string, Task<string>>? S3SvcReadIdAsync { get; init; }
    public Func<string, string, Task>? S3SvcUpdateIdAsync { get; init; }
    public Func<string, Task>? S3SvcDeleteIdAsync { get; init; }
    // Local storage access
    // Id is full path reference
    public Func<string, string, Task>? LocalSvcCreateIdAsync { get; init; }
    public Func<string, Task<string>>? LocalSvcReadIdAsync { get; init; }
    public Func<string, string, Task>? LocalSvcUpdateIdAsync { get; init; }
    public Func<string, Task>? LocalSvcDeleteIdAsync { get; init; }
    // _content access 
    // Id is something like "_content/library/somefile"
    // WASM implements this using HttpClient - assumes resource is under wwwroot
    // MAUI implements this using FileSystem.OpenAppPackageFileAsync(id)
    public Func<string, Task<string>>? ContentSvcReadIdAsync { get; init; }    // Http access - general http calls
    // Id is URL
    public Func<string, Task<string>>? HttpSvcReadIdAsync { get; init; }

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

            if (State != LzItemViewModelBaseState.New)
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
                    var s3Text = JsonConvert.SerializeObject(item);
                    await S3SvcCreateIdAsync(id!, s3Text!);
                    break;
                case StorageAPI.Http:
                    throw new Exception("HttpSvcCreateIdAsync is not supported.");
                case StorageAPI.Content:
                    throw new Exception("ContentSvcCreateIdAsync is not supported.");
                case StorageAPI.Local:
                    if (LocalSvcCreateIdAsync == null)
                        throw new Exception("LocalSvcCreateIdAsync not assigned.");
                    CheckId(id);
                    var localText = JsonConvert.SerializeObject(item);
                    await LocalSvcCreateIdAsync(id!,localText!);
                    break;
                case StorageAPI.Internal:
                    break;
            }

            UpdateData(item);
            State = LzItemViewModelBaseState.Current;
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
        var userMsg = "Can't load " + entityName;
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
                    var s3Text = await S3SvcReadIdAsync(id);
                    var s3Item = JsonConvert.DeserializeObject<TDTO>(s3Text);
                    UpdateData(s3Item!);
                    break;
                case StorageAPI.Http:
                    if (HttpSvcReadIdAsync == null)
                        throw new Exception("HttpSvcReadIdAsync not assigned.");
                    var httpText = await HttpSvcReadIdAsync(id);
                    var httpItem = JsonConvert.DeserializeObject<TDTO>(httpText);
                    UpdateData(httpItem!);
                    break;
                case StorageAPI.Content:
                    if (ContentSvcReadIdAsync == null)
                        throw new Exception("ContentSvcReadIdAsync not assigned.");
                    var contentText = await ContentSvcReadIdAsync(id);
                    var contextItem = JsonConvert.DeserializeObject<TDTO>(contentText);
                    UpdateData(contextItem!);
                    break;
                case StorageAPI.Local:
                    if (LocalSvcReadIdAsync == null)
                        throw new Exception("LocalSvcReadIdAsync not assigned.");
                    var localText = await LocalSvcReadIdAsync(id);
                    var localItem = JsonConvert.DeserializeObject<TDTO>(localText);
                    UpdateData(localItem!);
                    break;
                case StorageAPI.Internal:
                    break;
                
            }
            
            LastNotificationTick = UpdatedAt;
            State = LzItemViewModelBaseState.Current;

            if (AutoLoadChildren)
                return await ReadChildrenAsync(forceload: true, storageAPI);

            return (true, string.Empty);
        }
        catch (Exception ex)
        {

            return (false, Log(userMsg + " " + MethodBase.GetCurrentMethod()!, ex.Message));
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
                case StorageAPI.Internal:
                    throw new Exception("LocalSvcReadAsync not supported.");

            }
            LastNotificationTick = UpdatedAt;
            // Id = Data!.Id;
            State = LzItemViewModelBaseState.Current;
            IsLoaded = true;
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

            if (State != LzItemViewModelBaseState.Edit)
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
                    var s3Text = JsonConvert.SerializeObject(Data);
                    await S3SvcUpdateIdAsync(id!, s3Text);
                    break;
                case StorageAPI.Http:
                    throw new Exception("HttpSvcUpdateIdAsync is not supported.");
                case StorageAPI.Content:
                    throw new Exception("ContentSvcUpdateIdAsync is not supported.");
                case StorageAPI.Local:
                    if (LocalSvcUpdateIdAsync == null)
                        throw new Exception("LocalSvcUpdateIdAsync is not assigned.");
                    CheckId(id);
                    var localText = JsonConvert.SerializeObject(Data);
                    await LocalSvcUpdateIdAsync(id!,localText);
                    break;
                case StorageAPI.Internal:
                    break;
            }

            LastNotificationTick = UpdatedAt;
            State = LzItemViewModelBaseState.Current;
            return (true, string.Empty);
        }
        catch (Exception ex)
        {
            return (false, Log(MethodBase.GetCurrentMethod()!, ex.Message));
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

            if (State != LzItemViewModelBaseState.Current)
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
                case StorageAPI.Internal:
                    break;
            }

            State = LzItemViewModelBaseState.Deleted;
            Data = null;
            IsDirty = false;
            return(true,String.Empty);

        }
        catch (Exception ex)
        {
            return (false, Log(MethodBase.GetCurrentMethod()!, ex.Message));
        }
    }
    public virtual void OpenEdit(bool forceCopy = false)
    {
        if (!forceCopy && State == LzItemViewModelBaseState.Edit)
            return;

        if (State != LzItemViewModelBaseState.New)
            State = LzItemViewModelBaseState.Edit;
        MakeDataCopy();
    }
    public virtual Task OpenEditAsync(bool forceCopy = false)
    {
        if (!forceCopy && State == LzItemViewModelBaseState.Edit)
            return Task.CompletedTask;

        if(State != LzItemViewModelBaseState.New)
            State = LzItemViewModelBaseState.Edit;
        MakeDataCopy();
        return Task.CompletedTask;
    }
    public virtual async Task<(bool,string)> SaveEditAsync(string? id, StorageAPI storageAPI = StorageAPI.Default)
    {
        try
        {
            var (success, msg) =
                State == LzItemViewModelBaseState.New
                ? await CreateAsync(id, storageAPI)
                : await UpdateAsync(id, storageAPI);

            State = LzItemViewModelBaseState.Current;
            IsMerge = false;
            IsLoaded = true;
            return (success, msg);
        } 
        catch (Exception ex)
        {
            return (false, Log(MethodBase.GetCurrentMethod()!, ex.Message));
        }
    }
    public virtual (bool, string) CancelEdit()
    {
        if (State != LzItemViewModelBaseState.Edit && State != LzItemViewModelBaseState.New)
            return (false, Log(MethodBase.GetCurrentMethod()!, "No Active Edit"));

        State = (IsLoaded) ? LzItemViewModelBaseState.Current : LzItemViewModelBaseState.New;

        if (IsMerge)
            UpdateData(NotificationData!);
        else
            RestoreFromDataCopy();
        IsMerge = false;
        return (true, String.Empty);
    }
    public virtual async Task<(bool,string)> CancelEditAsync()
    {
        await Task.Delay(0);
        return CancelEdit();
    }
    public virtual bool Validate()
    {
        return true;
    }
    protected virtual void UpdateData(TDTO item)
    {
        // Updating data from JSON is not fast. Using 
        // Force.DeepCloner DeepCloneTo(Data) is not possible 
        // because it overwrites any event subscriptions.
        Data ??= new();
        var json = JsonConvert.SerializeObject(item);
        JsonConvert.PopulateObject(json, Data);
        IsDirty = false;
        this.RaisePropertyChanged(nameof(Data));
    }
    protected virtual void MakeDataCopy()
    {
        // Saving data using JSON is not fast. Using Force.DeepCloner
        // for DataCopy is not possible because the clone process 
        // fails if the source data has event subscriptions.
        Data ??= new();
        dataCopyJson = JsonConvert.SerializeObject(Data);
    }
    protected virtual void RestoreFromDataCopy()
    {
        // Restoring data from JSON is not fast. Using Force.DeepCloner 
        // DeepCloneTo(Data) is not possible because it overwrites any
        // event subscriptions.
        Data ??= new();
        JsonConvert.PopulateObject(dataCopyJson, Data);
    }
    public virtual async Task<(bool, string)> ReadChildrenAsync(bool forceload, StorageAPI storageAPI)
    {
        await Task.Delay(0);
        return (true, string.Empty);
    }

}
