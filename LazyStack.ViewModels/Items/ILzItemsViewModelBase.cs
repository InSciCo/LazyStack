namespace LazyStack.ViewModels
{
    public interface ILzItemsViewModelBase<TVM, TDTO, TModel> : ILzParentViewModel
        where TVM : class, ILzItemViewModelBase<TModel>
        where TDTO : class, new()
        where TModel : class, IRegisterObservables, TDTO, new()
        
    {
        bool AutoReadChildren { get; set; }
        bool CanAdd { get; set; }
        bool CanList { get; set; }
        TVM? CurrentViewModel { get; set; }
        string? Id { get; set; }
        bool IsChanged { get; set; }
        bool IsLoaded { get; set; }
        bool IsLoading { get; set; }
        long LastLoadTick { get; set; }
        TVM? LastViewModel { get; set; }

        Func<Task<ICollection<TDTO>>>? SvcReadList { get; init; }
        Func<string, Task<ICollection<TDTO>>>? SvcReadListId { get; init; }
        long UpdateCount { get; set; }
        Dictionary<string, TVM> ViewModels { get; set; }

        event NotifyCollectionChangedEventHandler? CollectionChanged;

        Task<(bool, string)> CancelCurrentViewModelEditAsync();
        Task<(bool, string)> ReadAsync(bool forceload = false, StorageAPI storageAPI = StorageAPI.Rest);
        Task<(bool, string)> ReadAsync(string parentId, bool forceload = false, StorageAPI storageAPI = StorageAPI.Rest);
        Task<(bool, string)> SaveCurrentViewModelAsync(string? id);
    }
}