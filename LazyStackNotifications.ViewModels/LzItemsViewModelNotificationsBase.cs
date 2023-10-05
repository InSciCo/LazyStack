namespace LazyStackNotifications.ViewModels;

public abstract class LzItemsViewModelNotificationsBase<TVM, TDTO, TModel> : 
    LzItemsViewModelBase<TVM, TDTO, TModel>, 
    INotifyCollectionChanged, 
    ILzItemsViewModelNotificationsBase<TVM, TDTO, TModel> where TDTO : class, new()
    where TModel : class, TDTO, IRegisterObservables, new()
    where TVM : class, ILzItemViewModelBase<TModel>
{
    public LzItemsViewModelNotificationsBase() 
    {
        this.WhenAnyValue(x => x.NotificationsSvc!.Notification!)
            .WhereNotNull()
            .Where(x => x.PayloadParentId == ParentId)
            .Subscribe(async (x) => await UpdateFromNotificationAsync(x));
    }
    public ILzNotificationSvc? NotificationsSvc { get; init; }
    public string ParentId { get; set; } = string.Empty;
    [Reactive] public long NotificationLastTick { get; set; }
    public virtual async Task UpdateFromNotificationAsync(LzNotification notification)
    {
        await Task.Delay(0);
        return;
    }
}
