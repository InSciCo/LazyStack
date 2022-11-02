using Microsoft.Extensions.DependencyInjection;
using ReactiveUI.Fody.Helpers;
using ReactiveUI;
using System.Reflection;
using Amazon.Runtime.Internal.Transform;
using System.Linq;

namespace LazyStack.ViewModels;



public class ItemsViewModelBase<TVM> : LzViewModelBase
    where TVM : class, IItemViewModelBase
{

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
    public bool CanList { get; set; } = true;

    public virtual void CancelCurrentViewModelAdd()
    {
        if (LastViewModel?.Id != null && ViewModels.ContainsKey(LastViewModel.Id!))
            CurrentViewModel = LastViewModel;
        else
            CurrentViewModel = null;
    }
    public virtual async Task<(bool,string)> CancelCurrentViewModelEditAsync()
    {
        try 
        { 
            if (CurrentViewModel == null)
                return (false, "CurrentViewModel is null");

            if (CurrentViewModel.State == ItemViewModelBaseState.New)
                return (true, String.Empty);

            if (CurrentViewModel.State != ItemViewModelBaseState.Edit)
                throw new Exception("State != Edit");

            await CurrentViewModel.ReadAsync(CurrentViewModel.Id!);
            CurrentViewModel.State = ItemViewModelBaseState.Current;
            return (true, String.Empty);
        }
        catch (Exception ex)
        {
            return (false, Log(string.Empty, ex.Message));
        }
    }

    public virtual async Task<(bool,string)> SaveCurrentViewModelAsync()
    {
        if (CurrentViewModel == null)
            return (false, "CurrentViewModel is null");

        var isAdd = CurrentViewModel.State == ItemViewModelBaseState.New;
        var (success, msg) = await CurrentViewModel.SaveAsync();
        if (success && isAdd)
        {
            if (CurrentViewModel.Id == null)
                throw new Exception("ItemViewModel.Id is null");
            ViewModels.TryAdd(CurrentViewModel.Id, CurrentViewModel);
        }

        return (success,msg);
    }

}
