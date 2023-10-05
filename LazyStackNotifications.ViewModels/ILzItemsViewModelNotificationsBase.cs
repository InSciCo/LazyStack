namespace LazyStackNotifications.ViewModels;

public interface ILzItemsViewModelNotificationsBase<TVM, TDTO, TModel> : ILzParentViewModel
        where TVM : class, ILzItemViewModelBase<TModel>
        where TDTO : class, new()
        where TModel : class, IRegisterObservables, TDTO, new()
{
    public ILzNotificationSvc? NotificationsSvc { get; init; }
    public long NotificationLastTick { get; set; }
    public Task UpdateFromNotificationAsync(LzNotification notificaiton);
}
