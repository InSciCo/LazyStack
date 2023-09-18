namespace LazyStack.ViewModels;

public interface ILzItemViewModelBase
{
    public string UpdateTickField { get; set; }
    public string? Id { get; }
    public long UpdatedAt { get; }
    public LzItemViewModelBaseState State { get; set; }
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
    public bool IsDirty { get; set; }

    public ILzParentViewModel? ParentViewModel { get; set; }

    public Task<(bool, string)> CreateAsync(string? id,StorageAPI storageAPI = StorageAPI.Default);
    public Task<(bool, string)> ReadAsync(string id, StorageAPI storageAPI = StorageAPI.Default);
    public Task<(bool, string)> ReadAsync(StorageAPI storageAPI = StorageAPI.Default);
    public Task<(bool, string)> UpdateAsync(string? id, StorageAPI storageAPI = StorageAPI.Default);
    public Task<(bool, string)> SaveEditAsync(string? id, StorageAPI storageAPI = StorageAPI.Default);
    public Task<(bool, string)> DeleteAsync(string id, StorageAPI storageAPI = StorageAPI.Default); 
    public Task<(bool, string)> CancelEditAsync();
    public Task<(bool, string)> ReadChildrenAsync(bool forceload, StorageAPI storageAPI);
    
}
