using System.Reflection;
using ReactiveUI.Fody.Helpers;
using Force.DeepCloner;
using LazyStackAuthV2;
using ReactiveUI;

namespace LazyStack.ViewModels;
 
public enum ItemViewModelBaseState
{
    New,
    Edit,
    Current,
    Deleted
}

public interface IItemViewModelBase
{
    public string? Id { get; set; }
    public ItemViewModelBaseState State { get; set; }
    public Task<(bool, string)> CreateAsync();
    public Task<(bool, string)> ReadAsync(string id);
    public Task<(bool, string)> ReadAsync();
    public Task<(bool, string)> UpdateAsync();
    public Task<(bool, string)> SaveEditAsync();
    public Task<(bool, string)> DeleteAsync(); 
    public Task<(bool, string)> CancelEditAsync();
    public bool CanCreate { get; set; }
    public bool CanRead { get; set; }   
    public bool CanUpdate { get; set; }    
    public bool CanDelete { get; set; } 
    public bool IsLoaded { get; set; }  
    public bool ActiveEdit { get; set; }    
    public bool DataCopied { get; set; }
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
public class ItemViewModelBase<TDTO, TModel> : LzViewModelBase, IItemViewModelBase
    where TDTO : class, new()
    where TModel : class, TDTO, IId, new()
{
    public ItemViewModelBase()
    {
        CanCreate = true;
        CanRead= true;
        CanUpdate= true;
        CanDelete= true;
        IsLoaded = false;
        ActiveEdit= false;

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
    [Reactive] public ItemViewModelBaseState State { get; set; }
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
    [Reactive] public bool ActiveEdit { get; set; }
    [Reactive] public bool DataCopied { get; set; }
    [ObservableAsProperty] public bool IsNew { get; }
    [ObservableAsProperty] public bool IsEdit { get; }
    [ObservableAsProperty] public bool IsCurrent { get; }
    [ObservableAsProperty] public bool IsDeleted { get; }

    public virtual async Task<(bool, string)> CreateAsync()
    {
        try
        {
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
    public virtual async Task<(bool,string)> DeleteAsync(string Id)
    {
        try
        {
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
    public virtual Task OpenEditAsync(bool copyData = true)
    {
        ActiveEdit = true;
        if(State != ItemViewModelBaseState.New)
            State = ItemViewModelBaseState.Edit;
        if (copyData)
        {
            DataCopy ??= new();
            Data.DeepCloneTo(DataCopy);
            DataCopied = true;
        }
        else
        {
            DataCopy = null;
            DataCopied = false;
        }
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

            ActiveEdit= false;
            State = ItemViewModelBaseState.Current;

            return (success, msg);
        } 
        catch (Exception ex)
        {
            return (false, Log(MethodBase.GetCurrentMethod()!, ex.Message));
        }
    }
    public virtual async Task<(bool,string)> CancelEditAsync()
    {
        if (!ActiveEdit)
            return (false, Log(MethodBase.GetCurrentMethod()!, "No Active Edit"));

        ActiveEdit = false;
        State = (IsLoaded) ? ItemViewModelBaseState.Current : ItemViewModelBaseState.New;

        if (DataCopied)
        {
            DataCopy.DeepCloneTo(Data);
            DataCopied = false;
            return (true,String.Empty);
        }
        try
        {
            if (IsLoaded)
            {
                if (SvcReadIdAsync != null)
                {
                    var data = await SvcReadIdAsync(Data!.Id!);
                }
                else if (SvcReadAsync != null)
                {
                    var data = await SvcReadAsync();
                }
            }
            return (true,String.Empty); 
        } catch (Exception ex) 
        {
            return (false, Log(MethodBase.GetCurrentMethod()!,ex.Message)); 
        }
    }
    public virtual bool Validate()
    {
        return true;
    }

}
