using Microsoft.Extensions.DependencyInjection;
using ReactiveUI.Fody.Helpers;
using ReactiveUI;
using System.Reflection;
using Amazon.Runtime.Internal.Transform;

namespace LazyStack.ViewModels;


public interface IItemsViewModelBase
{
    public void CancelAdd();
    public void AddViewModel(object viewModelObj);
}

public class ItemsViewModelBase<TParent,TVM> : LzViewModelBase, IItemsViewModelBase
    where TParent : class
    where TVM : class, IId, IItemViewModelState
{

    //public ItemsViewModelBase(TParent parentViewModel)
    //{
    //    ParentViewModel = parentViewModel;
    //}

    public string? Id { get; set; }
    public TParent ParentViewModel { get; set; }
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
    [Reactive] public bool IsChanged { get; private set; }
    public bool CanList { get; set; } = true;
    public virtual Task<(bool,string)> Init(object parentViewModel)
    {
        return Task.FromResult((true, string.Empty));
    }
    public virtual Task<(bool, string)> ReadAsync()
    {
        return Task.FromResult((true, string.Empty));
    }

    public virtual void CancelAdd()
    {
        if (LastViewModel?.Id != null && ViewModels.ContainsKey(LastViewModel.Id!))
            CurrentViewModel = LastViewModel;
        else
            CurrentViewModel = null;
    }
    /// <summary>
    /// AddViewModel() takes an object arg because C# Generics don't have
    /// an easy way to specifying the type of "self" in the conditions 
    /// statement. There may be a way of doing it, but I've run out of time
    /// to work on that. This call can fail at runtime if you pass the 
    /// wrong type of object. This is unlikely in the normal uses of this
    /// method, but it is possible.
    /// </summary>
    /// <param name="viewModelObj"></param>
    /// <exception cref="Exception"></exception>
    public virtual void AddViewModel(object viewModelObj)
    {
        var viewModel = viewModelObj as TVM;
        if (viewModel == null)
            throw new Exception("Can't convert object to required type");

        CurrentViewModel = viewModel;
        if (viewModel.State == ItemViewModelBaseState.Current)
        {
            if (viewModel.Id == null)
                throw new Exception("ItemViewModel.Id is null");
            ViewModels.TryAdd(viewModel.Id, viewModel);
        }
    }

    public virtual async Task InitAsync()
    {

        await ReadAsync();
    }

}
