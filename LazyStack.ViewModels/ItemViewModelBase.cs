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

public interface IItemBreadCrumb
{
    string? BreadCrumbName { get; }
    object? BreadCrumbParent { get; }
    string ViewModelType { get; }
}

public interface IItemViewModelState
{
    public ItemViewModelBaseState State { get; set; }
}

/// <summary>
/// ItemViewModelBase<T,TEdit> 
/// </summary>
/// <typeparam name="TDTO">DTO Type</typeparam>
/// <typeparam name="TModel">Model Type (extended model off of TDTO)</typeparam>
/// <typeparam name="TParent">ParentViewModel Type</typeparam>
public class ItemViewModelBase<TDTO, TModel, TParent> : LzViewModelBase, IId, IItemViewModelState, IItemBreadCrumb
    where TDTO : class, new()
    where TModel : class, TDTO, IId, new()
    where TParent : class, IItemsViewModelBase
{
    public IAuthProcess? AuthProcess { get; set; }
    [Reactive] public TModel? Data { get; set; }
    [Reactive] public ItemViewModelBaseState State { get; set; }
    [Reactive] public TParent? ParentViewModel { get; set; }
    public virtual string? BreadCrumbName => Id;
    public virtual object? BreadCrumbParent => ParentViewModel;
    public virtual string ViewModelType => "Item";
    public virtual string? Id
    {
        get { return (Data == null) ? string.Empty : Data.Id; }
        set { if(Data != null) Data.Id = value; }
    }

    protected Func<TDTO, Task<TDTO>>? SvcCreateAsync;
    protected Func<string, Task<TDTO>>? SvcReadAsync;
    protected Func<TDTO, Task<TDTO>>? SvcUpdateAsync;
    protected Func<string, Task>? SvcDeleteAsync;

    protected TDTO? InterimData { get; set; }

    public virtual Task<(bool,string)> Init(TParent parent)
    {
        return Task.FromResult((true, string.Empty));
    }

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
    public virtual async Task<(bool,string)> CancelEditAsync()
    {
        try
        {
            if (ParentViewModel == null)
                throw new Exception("ParentViewModel is null");

            if (State == ItemViewModelBaseState.New)
            {

                ParentViewModel.CancelAdd();
                Data = null;
                State = ItemViewModelBaseState.Deleted;
                return (true, String.Empty);
            }

            if (State != ItemViewModelBaseState.Edit)
                throw new Exception("State != Edit");

            await ReadAsync(Data!.Id);
            State = ItemViewModelBaseState.Current;
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
            if (ParentViewModel == null)
                throw new Exception("ParentViewModel is null");

            var isAdd = State == ItemViewModelBaseState.New;
            var (success, msg) =
                isAdd
                ? await CreateAsync()
                : await UpdateAsync();

            if (success && isAdd)
            {
                ParentViewModel.AddViewModel(this);
            }
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
