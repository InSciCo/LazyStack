using System.Reflection;
using ReactiveUI.Fody.Helpers;
using Force.DeepCloner;
using LazyStackAuthV2;

namespace LazyStack.ViewModels;

/// <summary>
/// ItemViewModelBase<T,TEdit> 
/// </summary>
/// <typeparam name="T"></typeparam>
/// <typeparam name="TEdit"></typeparam>
public class ItemViewModelBase<T, TEdit> : LzViewModelBase
    where T : class,  new()
    where TEdit : class, T, IId, new()
{
    public IAuthProcess AuthProcess { get; init; }
    [Reactive] public TEdit Data { get; set; }
    [Reactive] public bool IsDirty { get; set; }
    [Reactive] public bool IsSaved { get; set; }
    [Reactive] public bool IsAdd { get; set; }
    public string Id { get; protected set; }

    protected Func<T, Task<T>>? SvcCreateAsync;
    protected Func<string, Task<T>>? SvcReadAsync;
    protected Func<T, Task<T>>? SvcUpdateAsync;
    protected Func<string, Task>? SvcDeleteAsync;

    protected T? InterimData { get; set; }

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

            if (!IsDirty)
                throw new Exception("Item not New. Can't create.");

            if (SvcCreateAsync == null)
                throw new Exception("SvcCreateAsync not assigned.");

            if (Data == null)
                throw new Exception("Data not assigned");

            var item = (T)Data; 

            item = await SvcCreateAsync(item!);
            item.DeepCloneTo(Data);

            IsDirty = false;
            IsSaved = true;
            IsAdd = false;
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
            IsDirty = false;
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

            if (IsDirty)
                throw new Exception("Can't update a record with New status");

            if (SvcUpdateAsync == null)
                throw new Exception("SvcUpdateAsync is not assigned.");

            if (Data == null)
                throw new Exception("Data not assigned");

            var item = (T)Data;
            item = await SvcUpdateAsync(item);
            item.DeepCloneTo(Data!);
            IsDirty = false;
            IsSaved = true;
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

            if (IsDirty)
                throw new Exception("Can't update a record with New status");

            if (SvcDeleteAsync == null)
                throw new Exception("SvcDelete is not assigned.");

            await SvcDeleteAsync(Id);

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
            if (IsAdd)
            {
                return (true, String.Empty);
            }

            if (!IsDirty)
                throw new Exception("Buffer not dirty");

            await ReadAsync(Data!.Id);
            IsDirty = false;
            return(true,String.Empty);  
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
