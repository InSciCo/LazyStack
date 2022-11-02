using System.Reflection;
using ReactiveUI.Fody.Helpers;
using Force.DeepCloner;
using LazyStackAuthV2;

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
    public Task<(bool, string)> UpdateAsync();
    public Task<(bool, string)> SaveAsync();
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
    public IAuthProcess? AuthProcess { get; set; }
    [Reactive] public TModel? Data { get; set; }
    [Reactive] public ItemViewModelBaseState State { get; set; }
    public virtual string? Id
    {
        get { return (Data == null) ? string.Empty : Data.Id; }
        set { if(Data != null) Data.Id = value; }
    }

    protected Func<TDTO, Task<TDTO>>? SvcCreateAsync;
    protected Func<string, Task<TDTO>>? SvcReadAsync;
    protected Func<TDTO, Task<TDTO>>? SvcUpdateAsync;
    protected Func<string, Task>? SvcDeleteAsync;

    public bool CanCreate { get; set; } = true;
    public bool CanRead { get; set; } = true;
    public bool CanUpdate { get; set; } = true; 
    public bool CanDelete { get; set; } = true; 

    protected TDTO? InterimData { get; set; }

    //public virtual Task<(bool,string)> Init(TParent parent)
    //{
    //    return Task.FromResult((true, string.Empty));
    //}

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

            if (SvcReadAsync == null)
                throw new Exception("SvcReadAsync not assigned.");

            var item = await SvcReadAsync(id);
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

            if (Data == null)
                throw new Exception("Data not assigned");

            var item = (TDTO)Data;
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

            if (SvcDeleteAsync == null)
                throw new Exception("SvcDelete is not assigned.");

            await SvcDeleteAsync(Id);
            State = ItemViewModelBaseState.Deleted;
            Data = null;
            return(true,String.Empty);

        }
        catch (Exception ex)
        {
            return (false, Log(MethodBase.GetCurrentMethod()!, ex.Message));
        }
    }
    public virtual async Task<(bool,string)> SaveAsync()
    {
        try
        {
            var (success, msg) =
                State == ItemViewModelBaseState.New
                ? await CreateAsync()
                : await UpdateAsync();

            return (success, msg);
        } 
        catch (Exception ex)
        {
            return (false, Log(MethodBase.GetCurrentMethod()!, ex.Message));
        }
    }

    public virtual bool Validate()
    {
        return true;
    }


}
