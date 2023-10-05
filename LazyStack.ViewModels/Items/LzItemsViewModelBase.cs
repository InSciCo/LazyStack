namespace LazyStack.ViewModels;

/// <summary>
/// This class manages a list of ViewModels
/// TVM is the ViewModel Class in the list
/// TDTO is the data transfer object that the TVM model uses
/// TModel is the data model derived from the TDTO that the TVM presents to views.
/// Remember: During construction, Assign SvcReadChildren or SvcReadChildrenId, and entityName.
/// Also Implement NewViewModel()
/// </summary>
/// <typeparam name="TVM"></typeparam>
/// <typeparam name="TDTO"></typeparam>
/// <typeparam name="TModel"></typeparam>
public class LzItemsViewModelBase<TVM, TDTO, TModel> : LzViewModelBase, INotifyCollectionChanged, ILzItemsViewModelBase<TVM, TDTO, TModel> where TDTO : class, new()
    where TModel : class, TDTO, IRegisterObservables, new()
    where TVM : class,  ILzItemViewModelBase<TModel>
{
    public LzItemsViewModelBase()
    {
        CanList = true;
        CanAdd = true;
    }
    protected IAuthProcess? authProcess;
    public string? Id { get; set; }
    public Dictionary<string, TVM> ViewModels { get; set; } = new();
    private TVM? currentViewModel;
    public TVM? CurrentViewModel
    {
        get => currentViewModel;
        set
        {
            if (value != null && value != LastViewModel && value!.State != LzItemViewModelBaseState.New)
                LastViewModel = value;
            this.RaiseAndSetIfChanged(ref currentViewModel, value);
        }
    }
    [Reactive] public TVM? LastViewModel { get; set; }
    protected int changeCount;
    public event NotifyCollectionChangedEventHandler? CollectionChanged;
    public bool IsChanged
    {
        get => changeCount > 0;
        set => this.RaiseAndSetIfChanged(ref changeCount, changeCount + 1);
    }
    public bool AutoReadChildren { get; set; } = true;
    [Reactive] public bool IsLoaded { get; set; }
    [Reactive] public bool IsLoading { get; set; }
    [Reactive] public long LastLoadTick { get; set; }
    [Reactive] public bool CanList { get; set; }
    [Reactive] public bool CanAdd { get; set; }
    
    [Reactive] public virtual long UpdateCount { get; set; }
    public Func<string, Task<ICollection<TDTO>>>? SvcReadListId { get; init; }
    public Func<Task<ICollection<TDTO>>>? SvcReadList { get; init; }
    protected string entityName { get; set; } = string.Empty;
    protected virtual (TVM viewmodel, string id) NewViewModel(TDTO dto)
        => throw new NotImplementedException();
    public virtual async Task<(bool, string)> ReadAsync(bool forceload = false, StorageAPI storageAPI = StorageAPI.Rest)
        => await ReadAsync(string.Empty, forceload, storageAPI);
    public virtual async Task<(bool, string)> ReadAsync(string parentId, bool forceload = false, StorageAPI storageAPI = StorageAPI.Rest)
    {
        if (storageAPI == StorageAPI.Default)
            storageAPI = (storageAPI == StorageAPI.Default)
                ? StorageAPI.Rest
                : storageAPI;

        var userMsg = "Can't read " + entityName + " id:" + parentId;
        (bool success, string msg) result;
        try
        {
            if (storageAPI != StorageAPI.Rest)
                throw new Exception("Unsupported StorageAPI:" + storageAPI);

            CheckAuth(storageAPI);  

            if (SvcReadList == null && SvcReadListId == null)
                throw new Exception("SvcReadList function not assigned");
            IsLoading = true;
            var items = (SvcReadListId != null)
                ? await SvcReadListId(parentId)
                : await SvcReadList!();
            var tasks = new List<Task<(bool success, string msg)>>();
            foreach (var item in items)
            {
                var (vm, itemMsg) = NewViewModel(item);
                var id = vm.Id;
                if (id is null)
                    throw new Exception("NewViewModel return null id");
                if (!ViewModels!.ContainsKey(id))
                    ViewModels!.Add(id, vm);
                else
                    ViewModels![id] = vm;
                vm.State = LzItemViewModelBaseState.Current;
                if (AutoReadChildren)
                    tasks.Add(ViewModels![id].ReadChildrenAsync(forceload, storageAPI));
            }
            await Task.WhenAll(tasks);
            result = tasks.Where(x => x.Result.success == false).Select(x => x.Result).FirstOrDefault((true, string.Empty));
            IsLoaded = result.success;
            return result;
        }
        catch (Exception ex)
        {
            return (false, Log(userMsg, ex.Message));
        }
        finally { IsLoading = false; }
    }
    public virtual async Task<(bool, string)> CancelCurrentViewModelEditAsync()
    {
        try
        {
            if (CurrentViewModel == null)
                return (false, "CurrentViewModel is null");
            if (CurrentViewModel.State != LzItemViewModelBaseState.New && CurrentViewModel.State != LzItemViewModelBaseState.Edit)
                throw new Exception("State != Edit && State != New");
            await CurrentViewModel.CancelEditAsync();
            if (CurrentViewModel.State == LzItemViewModelBaseState.New)
            {
                if (LastViewModel?.Id != null && ViewModels.ContainsKey(LastViewModel.Id!))
                    CurrentViewModel = LastViewModel;
                else
                    CurrentViewModel = null;
                return (true, string.Empty);
            }
            return (true, String.Empty);
        }
        catch (Exception ex)
        {
            return (false, Log(string.Empty, ex.Message));
        }
    }
    public virtual async Task<(bool, string)> SaveCurrentViewModelAsync(string? id)
    {
        if (CurrentViewModel == null)
            return (false, "CurrentViewModel is null");
        var isAdd = CurrentViewModel.State == LzItemViewModelBaseState.New;
        var (success, msg) = await CurrentViewModel.SaveEditAsync(id);
        if (success && isAdd)
        {
            if (CurrentViewModel.Id == null)
                throw new Exception("ItemViewModel.Id is null");
            ViewModels.TryAdd(CurrentViewModel.Id, CurrentViewModel);
        }
        return (success, msg);
    }
    private void CheckAuth(StorageAPI storageAPI)
    {
        // Check for Auth
        switch (storageAPI)
        {
            case StorageAPI.Rest:
            case StorageAPI.S3:
                if (authProcess == null)
                    throw new Exception("AuthProcess not assigned");

                if (authProcess.IsNotSignedIn)
                    throw new Exception("Not signed in.");
                break;
            default:
                break;
        }
    }
}
